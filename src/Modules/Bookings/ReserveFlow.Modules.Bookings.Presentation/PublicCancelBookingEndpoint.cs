using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.CancelPublicBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class PublicCancelBookingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/public/bookings/{publicReference}/cancel", Handle)
            .WithTags("Public Bookings")
            .WithName("PublicCancelBooking");
    }

    private static async Task<Results<Ok<BookingResponse>, NotFound>> Handle(
        string publicReference,
        PublicCancelBookingRequest request,
        ICommandHandler<CancelPublicBookingCommand, BookingResponse?> handler,
        CancellationToken cancellationToken)
    {
        BookingResponse? response = await handler.Handle(
            new CancelPublicBookingCommand(
                publicReference,
                request.AccessToken,
                DateTimeOffset.UtcNow),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
