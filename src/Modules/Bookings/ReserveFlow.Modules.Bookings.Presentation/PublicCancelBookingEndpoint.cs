using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.CancelPublicBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class PublicCancelBookingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/public/tenants/{tenantSlug}/bookings/{publicReference}/cancel", Handle)
            .WithTags("Public Bookings")
            .WithName("PublicCancelBooking");
    }

    private static async Task<Results<Ok<BookingResponse>, NotFound>> Handle(
        string tenantSlug,
        string publicReference,
        PublicCancelBookingRequest request,
        ITenantSlugResolver tenantSlugResolver,
        ICommandHandler<CancelPublicBookingCommand, BookingResponse?> handler,
        CancellationToken cancellationToken)
    {
        Guid? tenantId = await tenantSlugResolver.ResolveTenantIdAsync(tenantSlug, cancellationToken);

        if (tenantId is null)
        {
            return TypedResults.NotFound();
        }

        BookingResponse? response = await handler.Handle(
            new CancelPublicBookingCommand(
                tenantId.Value,
                publicReference,
                request.AccessToken,
                DateTimeOffset.UtcNow),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
