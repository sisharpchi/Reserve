using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.MarkBookingAsNoShow;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class AdminMarkBookingAsNoShowEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/bookings/{bookingId:guid}/no-show", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Bookings")
            .WithName("AdminMarkBookingAsNoShow");
    }

    private static async Task<Results<Ok<BookingResponse>, NotFound>> Handle(
        Guid bookingId,
        ICommandHandler<MarkBookingAsNoShowCommand, BookingResponse?> handler,
        CancellationToken cancellationToken)
    {
        BookingResponse? response = await handler.Handle(
            new MarkBookingAsNoShowCommand(bookingId, DateTimeOffset.UtcNow),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
