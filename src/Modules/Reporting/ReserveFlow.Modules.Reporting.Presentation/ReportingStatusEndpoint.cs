using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Reporting.Presentation;

internal sealed class ReportingStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/reporting/status", () =>
            Results.Ok(new
            {
                Module = "Reporting",
                Schema = "reporting",
                ReadModel = "daily_booking_reports",
                Status = "Configured with daily booking report read model, record/query slices, and EF Core DbContext"
            }))
            .WithTags("Reporting");
    }
}
