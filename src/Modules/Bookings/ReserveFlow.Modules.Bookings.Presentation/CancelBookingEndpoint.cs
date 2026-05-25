using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.CancelBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class CancelBookingEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/bookings/{bookingId:guid}/cancel", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Bookings")
            .WithName("CancelBooking");
    }

    private static async Task<Ok<BookingResponse>> Handle(
        Guid bookingId,
        ICommandHandler<CancelBookingCommand, BookingResponse> handler,
        CancellationToken cancellationToken)
    {
        BookingResponse response = await handler.Handle(
            new CancelBookingCommand(bookingId, DateTimeOffset.UtcNow, EnforcePolicy: false),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
