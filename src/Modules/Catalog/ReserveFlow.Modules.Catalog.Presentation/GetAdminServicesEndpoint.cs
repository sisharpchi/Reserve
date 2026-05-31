using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Application.Services.GetServices;

namespace ReserveFlow.Modules.Catalog.Presentation;

internal sealed class GetAdminServicesEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/services", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .RequireTenantAccess()
            .WithTags("Catalog")
            .WithName("GetAdminServices");
    }

    private static async Task<Ok<IReadOnlyList<ServiceResponse>>> Handle(
        Guid tenantId,
        IQueryHandler<GetServicesQuery, IReadOnlyList<ServiceResponse>> handler,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ServiceResponse> response = await handler.Handle(
            new GetServicesQuery(tenantId),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
