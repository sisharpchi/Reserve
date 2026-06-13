using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.CompleteBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class AdminCompleteBookingEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/bookings/{bookingId:guid}/complete", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Bookings")
            .WithName("AdminCompleteBooking");
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
