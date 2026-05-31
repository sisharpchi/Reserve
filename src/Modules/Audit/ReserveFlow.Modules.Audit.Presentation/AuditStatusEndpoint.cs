using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Audit.Presentation;

internal sealed class AuditStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/audit/status", () =>
            Results.Ok(new
            {
                Module = "Audit",
                Schema = "audit",
                Table = "audit_logs",
                Status = "Configured with audit log aggregate, record slice, and EF Core DbContext"
            }))
            .WithTags("Audit");
    }
}
