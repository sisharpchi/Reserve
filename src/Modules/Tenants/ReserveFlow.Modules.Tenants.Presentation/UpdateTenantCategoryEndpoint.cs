using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.TenantCategories;
using ReserveFlow.Modules.Tenants.Application.TenantCategories.UpdateTenantCategory;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class UpdateTenantCategoryEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/platform/categories/{categoryId:guid}", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Platform Categories")
            .WithName("UpdateTenantCategory");
    }

    private static async Task<Results<Ok<TenantCategoryResponse>, NotFound>> Handle(
        Guid categoryId,
        UpdateTenantCategoryRequest request,
        ICommandHandler<UpdateTenantCategoryCommand, TenantCategoryResponse?> handler,
        CancellationToken cancellationToken)
    {
        TenantCategoryResponse? response = await handler.Handle(
            new UpdateTenantCategoryCommand(
                categoryId,
                request.Name,
                request.Slug,
                request.SortOrder),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
