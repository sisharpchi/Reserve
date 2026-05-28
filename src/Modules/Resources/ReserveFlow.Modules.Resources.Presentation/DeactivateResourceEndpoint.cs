using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Application.Resources.DeactivateResource;

namespace ReserveFlow.Modules.Resources.Presentation;

internal sealed class DeactivateResourceEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/admin/resources/{resourceId:guid}", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Resources")
            .WithName("DeactivateResource");
    }

    private static async Task<IResult> Handle(
        Guid resourceId,
        ICommandHandler<DeactivateResourceCommand, ResourceResponse?> handler,
        CancellationToken cancellationToken)
    {
        ResourceResponse? response = await handler.Handle(
            new DeactivateResourceCommand(resourceId),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
