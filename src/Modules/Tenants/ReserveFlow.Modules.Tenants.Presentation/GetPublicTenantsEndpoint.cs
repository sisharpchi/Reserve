using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Application.Tenants.GetPublicTenants;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class GetPublicTenantsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/tenants", Handle)
            .WithTags("Public Tenants")
            .WithName("GetPublicTenants");
    }

    private static async Task<Ok<IReadOnlyList<TenantResponse>>> Handle(
        Guid? categoryId,
        string? search,
        IQueryHandler<GetPublicTenantsQuery, IReadOnlyList<TenantResponse>> handler,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<TenantResponse> response = await handler.Handle(
            new GetPublicTenantsQuery(categoryId, search),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
