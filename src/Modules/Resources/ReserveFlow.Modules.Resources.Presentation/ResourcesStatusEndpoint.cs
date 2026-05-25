using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Resources.Presentation;

internal sealed class ResourcesStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/resources/status", () =>
            Results.Ok(new
            {
                Module = "Resources",
                Schema = "resources",
                Status = "Configured with resource aggregate, EF Core DbContext, tenant-scoped indexes, and admin/public endpoints"
            }))
            .WithTags("Resources");
    }
}
