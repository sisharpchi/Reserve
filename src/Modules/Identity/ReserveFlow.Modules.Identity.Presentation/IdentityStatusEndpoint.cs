using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Identity.Presentation;

internal sealed class IdentityStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/status", () =>
            Results.Ok(new
            {
                Module = "Identity",
                Provider = "Keycloak",
                Schema = "identity",
                Status = "Configured with Keycloak JWT authentication, current-user endpoint, and EF Core identity mappings"
            }))
            .WithTags("Identity");
    }
}
