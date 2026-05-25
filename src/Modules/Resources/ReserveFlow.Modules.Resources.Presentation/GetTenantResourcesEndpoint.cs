using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Application.Resources.GetActiveResources;

namespace ReserveFlow.Modules.Resources.Presentation;

internal sealed class GetTenantResourcesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/tenants/{tenantId:guid}/resources", Handle)
            .WithTags("Public Resources")
            .WithName("GetTenantResources");
    }

    private static async Task<Ok<IReadOnlyList<ResourceResponse>>> Handle(
        Guid tenantId,
        IQueryHandler<GetActiveResourcesQuery, IReadOnlyList<ResourceResponse>> handler,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ResourceResponse> response = await handler.Handle(
            new GetActiveResourcesQuery(tenantId),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
