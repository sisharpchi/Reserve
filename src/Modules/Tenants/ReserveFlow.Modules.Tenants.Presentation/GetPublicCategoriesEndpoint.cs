using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.TenantCategories;
using ReserveFlow.Modules.Tenants.Application.TenantCategories.GetPublicTenantCategories;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class GetPublicCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/categories", Handle)
            .WithTags("Public Tenants")
            .WithName("GetPublicCategories");
    }

    private static async Task<Ok<IReadOnlyList<TenantCategoryResponse>>> Handle(
        IQueryHandler<GetPublicTenantCategoriesQuery, IReadOnlyList<TenantCategoryResponse>> handler,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<TenantCategoryResponse> response = await handler.Handle(
            new GetPublicTenantCategoriesQuery(),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
