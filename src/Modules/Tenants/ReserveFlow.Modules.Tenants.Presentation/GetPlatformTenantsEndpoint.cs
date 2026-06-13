using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Application.Tenants.GetPlatformTenants;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class GetPlatformTenantsEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/platform/tenants", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Tenants")
            .WithName("GetPlatformTenants");
    }

    private static async Task<Ok<PagedResponse<TenantResponse>>> Handle(
        int? pageNumber,
        int? pageSize,
        Guid? categoryId,
        string? search,
        string? status,
        string? sortBy,
        string? sortDirection,
        IQueryHandler<GetPlatformTenantsQuery, PagedResponse<TenantResponse>> handler,
        CancellationToken cancellationToken)
    {
        PagedResponse<TenantResponse> response = await handler.Handle(
            new GetPlatformTenantsQuery(pageNumber, pageSize, categoryId, search, status, sortBy, sortDirection),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
