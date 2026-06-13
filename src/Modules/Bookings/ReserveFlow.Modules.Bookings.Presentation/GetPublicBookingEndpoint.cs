using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.GetPublicBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class GetPublicBookingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/tenants/{tenantSlug}/bookings/{publicReference}", Handle)
            .WithTags("Public Bookings")
            .WithName("GetPublicBooking");
    }

    private static async Task<Results<Ok<BookingResponse>, NotFound>> Handle(
        string tenantSlug,
        string publicReference,
        string accessToken,
        ITenantSlugResolver tenantSlugResolver,
        IQueryHandler<GetPublicBookingQuery, BookingResponse?> handler,
        CancellationToken cancellationToken)
    {
        Guid? tenantId = await tenantSlugResolver.ResolveTenantIdAsync(tenantSlug, cancellationToken);

        if (tenantId is null)
        {
            return TypedResults.NotFound();
        }

        BookingResponse? response = await handler.Handle(
            new GetPublicBookingQuery(tenantId.Value, publicReference, accessToken),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
