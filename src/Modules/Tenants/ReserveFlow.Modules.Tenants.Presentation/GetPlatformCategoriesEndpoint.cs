using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.TenantCategories;
using ReserveFlow.Modules.Tenants.Application.TenantCategories.GetPlatformTenantCategories;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class GetPlatformCategoriesEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/platform/categories", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Platform Categories")
            .WithName("GetPlatformCategories");
    }

    private static async Task<Ok<PagedResponse<TenantCategoryResponse>>> Handle(
        int? pageNumber,
        int? pageSize,
        string? search,
        bool? isActive,
        string? sortBy,
        string? sortDirection,
        IQueryHandler<GetPlatformTenantCategoriesQuery, PagedResponse<TenantCategoryResponse>> handler,
        CancellationToken cancellationToken)
    {
        PagedResponse<TenantCategoryResponse> response = await handler.Handle(
            new GetPlatformTenantCategoriesQuery(pageNumber, pageSize, search, isActive, sortBy, sortDirection),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
