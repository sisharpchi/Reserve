using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Application.Tenants.UpdateTenant;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class UpdateTenantEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/platform/tenants/{tenantId:guid}", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Tenants")
            .WithName("UpdateTenant");
    }

    private static async Task<Results<Ok<TenantResponse>, NotFound>> Handle(
        Guid tenantId,
        UpdateTenantRequest request,
        ICommandHandler<UpdateTenantCommand, TenantResponse?> handler,
        CancellationToken cancellationToken)
    {
        TenantResponse? response = await handler.Handle(
            new UpdateTenantCommand(
                tenantId,
                request.Name,
                request.Slug,
                request.TimeZoneId,
                request.CategoryId),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
