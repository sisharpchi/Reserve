using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class TenantsStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/platform/tenants/status", () =>
            Results.Ok(new
            {
                Module = "Tenants",
                Tenancy = "Shared database + module schemas + tenant_id",
                Schema = "platform",
                Status = "Configured with domain model, EF Core DbContext, and platform/public tenant endpoints"
            }))
            .WithTags("Tenants");
    }
}
