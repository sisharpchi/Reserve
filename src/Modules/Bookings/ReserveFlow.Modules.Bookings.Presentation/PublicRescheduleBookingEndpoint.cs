using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.RateLimiting;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.PublicRescheduleBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class PublicRescheduleBookingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/public/tenants/{tenantSlug}/bookings/{publicReference}/reschedule", Handle)
            .RequireRateLimiting(RateLimitPolicies.PublicBooking)
            .WithTags("Public Bookings")
            .WithName("PublicRescheduleBooking");
    }

    private static async Task<Results<Ok<BookingResponse>, NotFound>> Handle(
        string tenantSlug,
        string publicReference,
        PublicRescheduleBookingRequest request,
        ITenantSlugResolver tenantSlugResolver,
        ICommandHandler<PublicRescheduleBookingCommand, BookingResponse?> handler,
        CancellationToken cancellationToken)
    {
        Guid? tenantId = await tenantSlugResolver.ResolveTenantIdAsync(tenantSlug, cancellationToken);

        if (tenantId is null)
        {
            return TypedResults.NotFound();
        }

        BookingResponse? response = await handler.Handle(
            new PublicRescheduleBookingCommand(
                tenantId.Value,
                publicReference,
                request.AccessToken,
                request.StartsAtUtc,
                request.EndsAtUtc,
                DateTimeOffset.UtcNow),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
