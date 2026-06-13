using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Application.Resources.GetActiveResources;

namespace ReserveFlow.Modules.Resources.Presentation;

internal sealed class GetTenantResourcesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/tenants/{tenantSlug}/resources", Handle)
            .WithTags("Public Resources")
            .WithName("GetTenantResources");
    }

    private static async Task<Results<Ok<IReadOnlyList<ResourceResponse>>, NotFound>> Handle(
        string tenantSlug,
        ITenantSlugResolver tenantSlugResolver,
        IQueryHandler<GetActiveResourcesQuery, IReadOnlyList<ResourceResponse>> handler,
        CancellationToken cancellationToken)
    {
        Guid? tenantId = await tenantSlugResolver.ResolveTenantIdAsync(tenantSlug, cancellationToken);

        if (tenantId is null)
        {
            return TypedResults.NotFound();
        }

        IReadOnlyList<ResourceResponse> response = await handler.Handle(
            new GetActiveResourcesQuery(tenantId.Value),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
