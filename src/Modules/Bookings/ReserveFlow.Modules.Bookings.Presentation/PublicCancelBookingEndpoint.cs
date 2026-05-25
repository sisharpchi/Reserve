using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.CancelBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class PublicCancelBookingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/public/bookings/{bookingId:guid}/cancel", Handle)
            .WithTags("Public Bookings")
            .WithName("PublicCancelBooking");
    }

    private static async Task<Ok<BookingResponse>> Handle(
        Guid bookingId,
        ICommandHandler<CancelBookingCommand, BookingResponse> handler,
        CancellationToken cancellationToken)
    {
        BookingResponse response = await handler.Handle(
            new CancelBookingCommand(bookingId, DateTimeOffset.UtcNow, EnforcePolicy: true),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
