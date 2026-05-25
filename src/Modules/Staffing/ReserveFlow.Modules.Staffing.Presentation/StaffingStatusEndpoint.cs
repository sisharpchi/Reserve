using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Staffing.Presentation;

internal sealed class StaffingStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/staffing/status", () =>
            Results.Ok(new
            {
                Module = "Staffing",
                Schema = "staffing",
                Status = "Configured with staff member aggregate, EF Core DbContext, tenant-scoped indexes, and admin/public endpoints"
            }))
            .WithTags("Staffing");
    }
}
