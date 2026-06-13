using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
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

    private static async Task<Ok<PagedResponse<ResourceResponse>>> Handle(
        Guid tenantId,
        int? pageNumber,
        int? pageSize,
        string? search,
        bool? isActive,
        string? resourceType,
        string? sortBy,
        string? sortDirection,
        IQueryHandler<GetResourcesQuery, PagedResponse<ResourceResponse>> handler,
        CancellationToken cancellationToken)
    {
        PagedResponse<ResourceResponse> response = await handler.Handle(
            new GetResourcesQuery(
                tenantId,
                pageNumber,
                pageSize,
                search,
                isActive,
                resourceType,
                sortBy,
                sortDirection),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
