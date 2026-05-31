using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Application.Resources.GetResource;

namespace ReserveFlow.Modules.Resources.Presentation;

internal sealed class GetAdminResourceEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/resources/{resourceId:guid}", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Resources")
            .WithName("GetAdminResource");
    }

    private static async Task<IResult> Handle(
        Guid resourceId,
        IQueryHandler<GetResourceQuery, ResourceResponse?> handler,
        CancellationToken cancellationToken)
    {
        ResourceResponse? response = await handler.Handle(
            new GetResourceQuery(resourceId),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
