using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Catalog.Presentation;

internal sealed class CatalogStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/catalog/status", () =>
            Results.Ok(new
            {
                Module = "Catalog",
                Schema = "catalog",
                Status = "Configured with service aggregate, EF Core DbContext, tenant-scoped service indexes, and admin/public endpoints"
            }))
            .WithTags("Catalog");
    }
}
