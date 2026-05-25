using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Application.Tenants.GetTenantBySlug;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class GetPublicTenantEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/tenants/{tenantSlug}", Handle)
            .WithTags("Public Tenants")
            .WithName("GetPublicTenant");
    }

    private static async Task<Results<Ok<TenantResponse>, NotFound>> Handle(
        string tenantSlug,
        IQueryHandler<GetTenantBySlugQuery, TenantResponse?> handler,
        CancellationToken cancellationToken)
    {
        TenantResponse? response = await handler.Handle(
            new GetTenantBySlugQuery(tenantSlug),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
