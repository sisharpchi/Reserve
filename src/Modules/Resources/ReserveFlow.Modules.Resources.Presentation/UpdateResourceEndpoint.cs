using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Application.Resources.UpdateResource;

namespace ReserveFlow.Modules.Resources.Presentation;

internal sealed class UpdateResourceEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/resources/{resourceId:guid}", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Resources")
            .WithName("UpdateResource");
    }

    private static async Task<IResult> Handle(
        Guid resourceId,
        UpdateResourceRequest request,
        ICommandHandler<UpdateResourceCommand, ResourceResponse?> handler,
        CancellationToken cancellationToken)
    {
        ResourceResponse? response = await handler.Handle(
            new UpdateResourceCommand(
                resourceId,
                request.Name,
                request.ResourceType,
                request.Capacity),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
