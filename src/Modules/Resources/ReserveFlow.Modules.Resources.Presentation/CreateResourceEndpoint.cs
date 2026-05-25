using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Application.Resources.CreateResource;

namespace ReserveFlow.Modules.Resources.Presentation;

internal sealed class CreateResourceEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/resources", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Resources")
            .WithName("CreateResource");
    }

    private static async Task<IResult> Handle(
        CreateResourceRequest request,
        ICommandHandler<CreateResourceCommand, ResourceResponse> handler,
        CancellationToken cancellationToken)
    {
        ResourceResponse response = await handler.Handle(
            new CreateResourceCommand(
                request.TenantId,
                request.Name,
                request.ResourceType,
                request.Capacity),
            cancellationToken);

        return TypedResults.Created($"/api/admin/resources/{response.Id}", response);
    }
}
