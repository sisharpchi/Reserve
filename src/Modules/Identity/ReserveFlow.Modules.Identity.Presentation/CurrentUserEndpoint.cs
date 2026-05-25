using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.RateLimiting;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Identity.Presentation;

internal sealed class CurrentUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/me", (HttpContext context) =>
        {
            var claims = context.User.Claims
                .Select(claim => new
                {
                    claim.Type,
                    claim.Value
                })
                .ToArray();

            return Results.Ok(new
            {
                IsAuthenticated = context.User.Identity?.IsAuthenticated ?? false,
                Name = context.User.Identity?.Name,
                Claims = claims
            });
        })
        .RequireRateLimiting(RateLimitPolicies.AuthContext)
        .RequireAuthorization()
        .WithTags("Identity");
    }
}
