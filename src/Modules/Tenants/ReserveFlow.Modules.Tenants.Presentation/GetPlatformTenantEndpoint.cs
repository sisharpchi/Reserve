using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Application.Tenants.GetTenant;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class GetPlatformTenantEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/platform/tenants/{tenantId:guid}", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Tenants")
            .WithName("GetPlatformTenant");
    }

    private static async Task<Results<Ok<TenantResponse>, NotFound>> Handle(
        Guid tenantId,
        IQueryHandler<GetTenantQuery, TenantResponse?> handler,
        CancellationToken cancellationToken)
    {
        TenantResponse? response = await handler.Handle(
            new GetTenantQuery(tenantId),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
