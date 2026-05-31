using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Integrations.Presentation;

internal sealed class IntegrationsStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/integrations/status", () =>
            Results.Ok(new
            {
                Module = "Integrations",
                Schema = "integrations",
                Inbox = "webhook_inbox_messages",
                Status = "Configured with duplicate-safe webhook inbox and EF Core DbContext"
            }))
            .WithTags("Integrations");
    }
}
