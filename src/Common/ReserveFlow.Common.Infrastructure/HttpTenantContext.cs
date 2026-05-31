using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using ReserveFlow.Common.Application.Abstractions;

namespace ReserveFlow.Common.Infrastructure;

public sealed class HttpTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    private HttpContext? HttpContext => httpContextAccessor.HttpContext;

    private ClaimsPrincipal? Principal => HttpContext?.User;

    public Guid? TenantId
    {
        get
        {
            string? value =
                GetHeaderValue("X-Tenant-Id") ??
                GetHeaderValue("X-ReserveFlow-Tenant-Id") ??
                GetClaimValue("tenant_id") ??
                GetClaimValue("tenant");

            return Guid.TryParse(value, out Guid tenantId)
                ? tenantId
                : null;
        }
    }

    public string? TenantSlug =>
        NormalizeOptional(
            GetHeaderValue("X-Tenant-Slug") ??
            GetHeaderValue("X-ReserveFlow-Tenant-Slug") ??
            GetClaimValue("tenant_slug"));

    public string? TimeZoneId =>
        NormalizeOptional(
            GetHeaderValue("X-Tenant-TimeZone") ??
            GetHeaderValue("X-ReserveFlow-TimeZone") ??
            GetClaimValue("tenant_timezone") ??
            GetClaimValue("tenant_tz"));

    public bool IsPlatformScope
    {
        get
        {
            string? platformScopeHeader = GetHeaderValue("X-Platform-Scope");

            if (IsTruthy(platformScopeHeader))
            {
                return true;
            }

            ClaimsPrincipal? principal = Principal;

            if (principal is null)
            {
                return false;
            }

            return KeycloakRoleClaims.HasAnyRole(principal, KeycloakRoles.PlatformAdmin) ||
                   principal.Claims
                       .Where(claim => claim.Type is "permissions" or "permission" or "scope" or "scp")
                       .SelectMany(claim => SplitClaimValues(claim.Value))
                       .Any(value => value.Equals("platform", StringComparison.OrdinalIgnoreCase) ||
                                     value.StartsWith("Platform.", StringComparison.OrdinalIgnoreCase));
        }
    }

    private string? GetHeaderValue(string headerName)
    {
        if (HttpContext?.Request.Headers.TryGetValue(headerName, out StringValues values) != true)
        {
            return null;
        }

        return NormalizeOptional(values.FirstOrDefault());
    }

    private string? GetClaimValue(string claimType)
    {
        return Principal?.FindFirst(claimType)?.Value;
    }

    private static bool IsTruthy(string? value)
    {
        return value is not null &&
               (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("yes", StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> SplitClaimValues(string value)
    {
        return value
            .Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item));
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
