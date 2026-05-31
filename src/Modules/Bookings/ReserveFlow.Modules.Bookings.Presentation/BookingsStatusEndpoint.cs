using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class BookingsStatusEndpoint : IEndpoint
{
    private static readonly string[] Lifecycle =
    [
        "Pending",
        "Confirmed",
        "Cancelled",
        "Rescheduled",
        "Completed",
        "NoShow",
        "Expired"
    ];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/bookings/status", () =>
            Results.Ok(new
            {
                Module = "Bookings",
                Lifecycle,
                Schema = "bookings",
                Status = "Configured with booking aggregate, booking policies, lifecycle events, create/cancel/reschedule/confirm/expire/complete/no-show slices, EF Core DbContext, and availability-ready indexes"
            }))
            .WithTags("Bookings");
    }
}
