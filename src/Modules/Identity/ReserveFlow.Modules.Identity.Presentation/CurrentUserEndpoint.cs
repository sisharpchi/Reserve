using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.RateLimiting;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Identity.Application.CurrentUser;

namespace ReserveFlow.Modules.Identity.Presentation;

internal sealed class CurrentUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/me", async (
            IQueryHandler<GetCurrentUserQuery, CurrentUserResponse> handler,
            CancellationToken cancellationToken) =>
        {
            CurrentUserResponse response = await handler.Handle(new GetCurrentUserQuery(), cancellationToken);

            return Results.Ok(response);
        })
        .RequireRateLimiting(RateLimitPolicies.AuthContext)
        .RequireAuthorization()
        .WithTags("Identity");
    }
}
