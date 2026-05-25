using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Application.Services.GetActiveServices;

namespace ReserveFlow.Modules.Catalog.Presentation;

internal sealed class GetTenantServicesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/tenants/{tenantId:guid}/services", Handle)
            .WithTags("Public Catalog")
            .WithName("GetTenantServices");
    }

    private static async Task<Ok<IReadOnlyList<ServiceResponse>>> Handle(
        Guid tenantId,
        IQueryHandler<GetActiveServicesQuery, IReadOnlyList<ServiceResponse>> handler,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ServiceResponse> response = await handler.Handle(
            new GetActiveServicesQuery(tenantId),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
