using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Application.Tenants.GetPlatformTenants;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class GetPlatformTenantsEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/platform/tenants", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Tenants")
            .WithName("GetPlatformTenants");
    }

    private static async Task<Ok<IReadOnlyList<TenantResponse>>> Handle(
        IQueryHandler<GetPlatformTenantsQuery, IReadOnlyList<TenantResponse>> handler,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<TenantResponse> response = await handler.Handle(
            new GetPlatformTenantsQuery(),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
