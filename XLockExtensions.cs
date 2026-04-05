using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace XLock.AspNetCore;

/// <summary>
/// Extension methods for registering x-lock services and middleware.
/// </summary>
public static class XLockExtensions
{
    /// <summary>
    /// Adds x-lock bot protection services to the dependency injection container.
    /// Registers <see cref="XLockOptions"/> and a named <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure <see cref="XLockOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddXLock(
        this IServiceCollection services,
        Action<XLockOptions> configure)
    {
        services.Configure(configure);
        services.AddHttpClient("XLock");
        return services;
    }

    /// <summary>
    /// Adds the x-lock bot protection middleware to the request pipeline.
    /// Should be called before routing / endpoint middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseXLock(this IApplicationBuilder app)
    {
        return app.UseMiddleware<XLockMiddleware>();
    }
}
