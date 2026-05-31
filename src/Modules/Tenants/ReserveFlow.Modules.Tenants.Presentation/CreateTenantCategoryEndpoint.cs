using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.TenantCategories;
using ReserveFlow.Modules.Tenants.Application.TenantCategories.CreateTenantCategory;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class CreateTenantCategoryEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/platform/categories", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Platform Categories")
            .WithName("CreateTenantCategory");
    }

    private static async Task<IResult> Handle(
        CreateTenantCategoryRequest request,
        ICommandHandler<CreateTenantCategoryCommand, TenantCategoryResponse> handler,
        CancellationToken cancellationToken)
    {
        TenantCategoryResponse response = await handler.Handle(
            new CreateTenantCategoryCommand(
                request.Name,
                request.Slug,
                request.SortOrder),
            cancellationToken);

        return TypedResults.Created($"/api/platform/categories/{response.Slug}", response);
    }
}
