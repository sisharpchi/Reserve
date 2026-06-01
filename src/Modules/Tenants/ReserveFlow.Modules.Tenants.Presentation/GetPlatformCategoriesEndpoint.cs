using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
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

    private static async Task<Ok<IReadOnlyList<TenantCategoryResponse>>> Handle(
        IQueryHandler<GetPlatformTenantCategoriesQuery, IReadOnlyList<TenantCategoryResponse>> handler,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<TenantCategoryResponse> response = await handler.Handle(
            new GetPlatformTenantCategoriesQuery(),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
