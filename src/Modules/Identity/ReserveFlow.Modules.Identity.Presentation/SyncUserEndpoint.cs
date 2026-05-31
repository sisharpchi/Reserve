using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.RateLimiting;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Identity.Application.Users.SyncUser;

namespace ReserveFlow.Modules.Identity.Presentation;

internal sealed class SyncUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/sync-user", async (
            ICommandHandler<SyncUserCommand, SyncUserResponse> handler,
            CancellationToken cancellationToken) =>
        {
            SyncUserResponse response = await handler.Handle(new SyncUserCommand(), cancellationToken);

            return Results.Ok(response);
        })
        .RequireRateLimiting(RateLimitPolicies.AuthContext)
        .RequireAuthorization()
        .WithTags("Identity")
        .WithName("SyncUser");
    }
}
