using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.GetPublicBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class GetPublicBookingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/bookings/{publicReference}", Handle)
            .WithTags("Public Bookings")
            .WithName("GetPublicBooking");
    }

    private static async Task<Results<Ok<BookingResponse>, NotFound>> Handle(
        string publicReference,
        string accessToken,
        IQueryHandler<GetPublicBookingQuery, BookingResponse?> handler,
        CancellationToken cancellationToken)
    {
        BookingResponse? response = await handler.Handle(
            new GetPublicBookingQuery(publicReference, accessToken),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
