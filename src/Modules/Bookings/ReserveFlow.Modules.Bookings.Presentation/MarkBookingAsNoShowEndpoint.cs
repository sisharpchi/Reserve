using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.MarkBookingAsNoShow;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class MarkBookingAsNoShowEndpoint : IEndpoint
{
    private const string StaffPolicy = "Staff";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/staff/bookings/{bookingId:guid}/no-show", Handle)
            .RequireAuthorization(StaffPolicy)
            .WithTags("Staff Bookings")
            .WithName("MarkBookingAsNoShow");
    }

    private static async Task<Ok<BookingResponse>> Handle(
        Guid bookingId,
        ICommandHandler<MarkBookingAsNoShowCommand, BookingResponse> handler,
        CancellationToken cancellationToken)
    {
        BookingResponse response = await handler.Handle(
            new MarkBookingAsNoShowCommand(bookingId, DateTimeOffset.UtcNow),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
