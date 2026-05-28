using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.RateLimiting;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Identity.Presentation;

internal sealed class CurrentUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/me", (ICurrentUser currentUser, ITenantContext tenantContext) => Results.Ok(new
        {
            IsAuthenticated = currentUser.KeycloakSubject is not null,
            currentUser.UserId,
            currentUser.KeycloakSubject,
            currentUser.Email,
            Permissions = currentUser.Permissions.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
            Tenant = new
            {
                tenantContext.TenantId,
                tenantContext.TenantSlug,
                tenantContext.TimeZoneId,
                tenantContext.IsPlatformScope
            }
        }))
        .RequireRateLimiting(RateLimitPolicies.AuthContext)
        .RequireAuthorization()
        .WithTags("Identity");
    }
}
