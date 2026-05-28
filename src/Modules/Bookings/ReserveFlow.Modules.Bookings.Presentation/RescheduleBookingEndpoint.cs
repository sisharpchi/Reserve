using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.RescheduleBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class RescheduleBookingEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/bookings/{bookingId:guid}/reschedule", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Bookings")
            .WithName("RescheduleBooking");
    }

    private static async Task<Results<Ok<BookingResponse>, NotFound>> Handle(
        Guid bookingId,
        RescheduleBookingRequest request,
        ICommandHandler<RescheduleBookingCommand, BookingResponse?> handler,
        CancellationToken cancellationToken)
    {
        BookingResponse? response = await handler.Handle(
            new RescheduleBookingCommand(
                bookingId,
                request.StartsAtUtc,
                request.EndsAtUtc),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
