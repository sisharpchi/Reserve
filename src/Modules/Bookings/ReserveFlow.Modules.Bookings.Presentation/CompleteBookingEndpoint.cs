using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.CompleteBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class CompleteBookingEndpoint : IEndpoint
{
    private const string StaffPolicy = "Staff";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/staff/bookings/{bookingId:guid}/complete", Handle)
            .RequireAuthorization(StaffPolicy)
            .WithTags("Staff Bookings")
            .WithName("CompleteBooking");
    }

    private static async Task<Results<Ok<BookingResponse>, NotFound>> Handle(
        Guid bookingId,
        ICommandHandler<CompleteBookingCommand, BookingResponse?> handler,
        CancellationToken cancellationToken)
    {
        BookingResponse? response = await handler.Handle(
            new CompleteBookingCommand(bookingId, DateTimeOffset.UtcNow),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
