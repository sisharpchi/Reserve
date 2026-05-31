using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Application.Resources.GetResources;

namespace ReserveFlow.Modules.Resources.Presentation;

internal sealed class GetAdminResourcesEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/resources", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .RequireTenantAccess()
            .WithTags("Resources")
            .WithName("GetAdminResources");
    }

    private static async Task<Ok<IReadOnlyList<ResourceResponse>>> Handle(
        Guid tenantId,
        IQueryHandler<GetResourcesQuery, IReadOnlyList<ResourceResponse>> handler,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ResourceResponse> response = await handler.Handle(
            new GetResourcesQuery(tenantId),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
