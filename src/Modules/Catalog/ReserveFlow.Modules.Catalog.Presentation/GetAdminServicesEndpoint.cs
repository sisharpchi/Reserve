using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
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

    private static async Task<Ok<PagedResponse<ServiceResponse>>> Handle(
        Guid tenantId,
        int? pageNumber,
        int? pageSize,
        string? search,
        bool? isActive,
        string? sortBy,
        string? sortDirection,
        IQueryHandler<GetServicesQuery, PagedResponse<ServiceResponse>> handler,
        CancellationToken cancellationToken)
    {
        PagedResponse<ServiceResponse> response = await handler.Handle(
            new GetServicesQuery(tenantId, pageNumber, pageSize, search, isActive, sortBy, sortDirection),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
