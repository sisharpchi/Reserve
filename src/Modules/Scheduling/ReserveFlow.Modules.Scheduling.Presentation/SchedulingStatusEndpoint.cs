using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Scheduling.Presentation;

internal sealed class SchedulingStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/scheduling/status", () =>
            Results.Ok(new
            {
                Module = "Scheduling",
                Schema = "scheduling",
                Status = "Configured with working hours persistence and availability slot generation"
            }))
            .WithTags("Scheduling");
    }
}
