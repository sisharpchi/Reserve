using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.ConfirmBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class ConfirmBookingEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/bookings/{bookingId:guid}/confirm", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Bookings")
            .WithName("ConfirmBooking");
    }

    private static async Task<Results<Ok<BookingResponse>, NotFound>> Handle(
        Guid bookingId,
        ICommandHandler<ConfirmBookingCommand, BookingResponse?> handler,
        CancellationToken cancellationToken)
    {
        BookingResponse? response = await handler.Handle(
            new ConfirmBookingCommand(bookingId, DateTimeOffset.UtcNow),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
