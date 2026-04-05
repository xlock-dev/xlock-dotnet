namespace XLock.AspNetCore;

/// <summary>
/// Configuration options for the x-lock bot protection middleware.
/// </summary>
public class XLockOptions
{
    /// <summary>
    /// Your x-lock site key (required). Obtain from https://x-lock.dev.
    /// </summary>
    public string SiteKey { get; set; } = "";

    /// <summary>
    /// Base URL of the x-lock verification API.
    /// </summary>
    public string ApiUrl { get; set; } = "https://api.x-lock.dev";

    /// <summary>
    /// When true, requests are allowed through if the x-lock API is unreachable
    /// or returns a non-403 error. When false, verification failures block the request.
    /// </summary>
    public bool FailOpen { get; set; } = true;

    /// <summary>
    /// List of path prefixes to protect. If empty, all POST requests are protected.
    /// Matching uses <see cref="PathString.StartsWithSegments"/>.
    /// </summary>
    public List<string> ProtectedPaths { get; set; } = new();
}
