using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.GetBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class GetAdminBookingEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/bookings/{bookingId:guid}", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Bookings")
            .WithName("GetAdminBooking");
    }

    private static async Task<IResult> Handle(
        Guid bookingId,
        ITenantContext tenantContext,
        IQueryHandler<GetBookingQuery, BookingResponse?> handler,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return TypedResults.BadRequest("Tenant context is required.");
        }

        BookingResponse? response = await handler.Handle(
            new GetBookingQuery(tenantId, bookingId),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
