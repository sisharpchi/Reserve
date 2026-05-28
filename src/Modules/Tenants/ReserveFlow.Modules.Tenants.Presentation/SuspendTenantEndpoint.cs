using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Application.Tenants.SuspendTenant;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class SuspendTenantEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/platform/tenants/{tenantId:guid}/suspend", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Tenants")
            .WithName("SuspendTenant");
    }

    private static async Task<Results<Ok<TenantResponse>, NotFound>> Handle(
        Guid tenantId,
        ICommandHandler<SuspendTenantCommand, TenantResponse?> handler,
        CancellationToken cancellationToken)
    {
        TenantResponse? response = await handler.Handle(
            new SuspendTenantCommand(tenantId),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
