using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Application.Services.GetActiveService;

namespace ReserveFlow.Modules.Catalog.Presentation;

internal sealed class GetTenantServiceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/tenants/{tenantSlug}/services/{serviceId:guid}", Handle)
            .WithTags("Public Catalog")
            .WithName("GetTenantService");
    }

    private static async Task<Results<Ok<ServiceResponse>, NotFound>> Handle(
        string tenantSlug,
        Guid serviceId,
        ITenantSlugResolver tenantSlugResolver,
        IQueryHandler<GetActiveServiceQuery, ServiceResponse?> handler,
        CancellationToken cancellationToken)
    {
        Guid? tenantId = await tenantSlugResolver.ResolveTenantIdAsync(tenantSlug, cancellationToken);

        if (tenantId is null)
        {
            return TypedResults.NotFound();
        }

        ServiceResponse? response = await handler.Handle(
            new GetActiveServiceQuery(tenantId.Value, serviceId),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
