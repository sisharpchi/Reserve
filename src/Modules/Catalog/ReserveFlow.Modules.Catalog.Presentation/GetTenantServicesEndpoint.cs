using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Application.Services.GetActiveServices;

namespace ReserveFlow.Modules.Catalog.Presentation;

internal sealed class GetTenantServicesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/tenants/{tenantSlug}/services", Handle)
            .WithTags("Public Catalog")
            .WithName("GetTenantServices");
    }

    private static async Task<Results<Ok<IReadOnlyList<ServiceResponse>>, NotFound>> Handle(
        string tenantSlug,
        ITenantSlugResolver tenantSlugResolver,
        IQueryHandler<GetActiveServicesQuery, IReadOnlyList<ServiceResponse>> handler,
        CancellationToken cancellationToken)
    {
        Guid? tenantId = await tenantSlugResolver.ResolveTenantIdAsync(tenantSlug, cancellationToken);

        if (tenantId is null)
        {
            return TypedResults.NotFound();
        }

        IReadOnlyList<ServiceResponse> response = await handler.Handle(
            new GetActiveServicesQuery(tenantId.Value),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
