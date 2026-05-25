using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.ExpirePendingBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class ExpirePendingBookingEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/bookings/{bookingId:guid}/expire", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Bookings")
            .WithName("ExpirePendingBooking");
    }

    private static async Task<Ok<BookingResponse>> Handle(
        Guid bookingId,
        ICommandHandler<ExpirePendingBookingCommand, BookingResponse> handler,
        CancellationToken cancellationToken)
    {
        BookingResponse response = await handler.Handle(
            new ExpirePendingBookingCommand(bookingId, DateTimeOffset.UtcNow),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
