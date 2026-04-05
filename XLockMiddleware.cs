using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XLock.AspNetCore;

/// <summary>
/// ASP.NET Core middleware that enforces x-lock bot protection on POST requests.
/// Verifies the <c>x-lock</c> request header against the x-lock API, routing
/// v3 session tokens and v1 one-shot tokens to their respective endpoints.
/// </summary>
public class XLockMiddleware
{
    private readonly RequestDelegate _next;
    private readonly XLockOptions _options;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<XLockMiddleware> _logger;

    public XLockMiddleware(
        RequestDelegate next,
        IOptions<XLockOptions> options,
        IHttpClientFactory httpFactory,
        ILogger<XLockMiddleware> logger)
    {
        _next = next;
        _options = options.Value;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip if no site key configured, non-POST, or path not protected
        if (string.IsNullOrEmpty(_options.SiteKey) ||
            context.Request.Method != "POST" ||
            !MatchesPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers["x-lock"].FirstOrDefault();

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Blocked by x-lock: missing token" });
            return;
        }

        try
        {
            var client = _httpFactory.CreateClient("XLock");
            client.Timeout = TimeSpan.FromSeconds(5);

            var path = context.Request.Path.ToString();
            string enforceUrl;
            object enforceBody;

            if (token.StartsWith("v3."))
            {
                var sessionId = token.Split('.')[1];
                enforceUrl = $"{_options.ApiUrl}/v3/session/enforce";
                enforceBody = new { sessionId, siteKey = _options.SiteKey, path };
            }
            else
            {
                enforceUrl = $"{_options.ApiUrl}/v1/enforce";
                enforceBody = new { token, siteKey = _options.SiteKey, path };
            }

            var response = await client.PostAsJsonAsync(enforceUrl, enforceBody);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var data = await response.Content.ReadFromJsonAsync<JsonElement>();
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Blocked by x-lock",
                    reason = data.TryGetProperty("reason", out var r) ? r.GetString() : null
                });
                return;
            }

            if (!response.IsSuccessStatusCode && !_options.FailOpen)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { error = "x-lock verification failed" });
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[x-lock] Enforcement error");
            if (!_options.FailOpen)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { error = "x-lock verification failed" });
                return;
            }
        }

        await _next(context);
    }

    private bool MatchesPath(PathString path)
    {
        if (_options.ProtectedPaths.Count == 0) return true;
        return _options.ProtectedPaths.Any(p => path.StartsWithSegments(p));
    }
}
