using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace ReserveFlow.Common.Infrastructure;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private const string ContentSecurityPolicy =
        "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";

    private const string PermissionsPolicy =
        "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            IHeaderDictionary headers = context.Response.Headers;

            AddHeaderIfMissing(headers, "X-Content-Type-Options", "nosniff");
            AddHeaderIfMissing(headers, "X-Frame-Options", "DENY");
            AddHeaderIfMissing(headers, "Referrer-Policy", "no-referrer");
            AddHeaderIfMissing(headers, "X-Permitted-Cross-Domain-Policies", "none");
            AddHeaderIfMissing(headers, "Permissions-Policy", PermissionsPolicy);

            if (!IsHtmlResponse(context.Response.ContentType))
            {
                AddHeaderIfMissing(headers, "Content-Security-Policy", ContentSecurityPolicy);
            }

            return Task.CompletedTask;
        });

        return next(context);
    }

    private static bool IsHtmlResponse(string? contentType)
    {
        return contentType is not null &&
               contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private static void AddHeaderIfMissing(
        IHeaderDictionary headers,
        string name,
        StringValues value)
    {
        if (!headers.ContainsKey(name))
        {
            headers[name] = value;
        }
    }
}

public static class SecurityHeadersApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
