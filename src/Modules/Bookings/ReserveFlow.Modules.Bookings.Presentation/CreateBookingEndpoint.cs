using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.RateLimiting;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.CreateBooking;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class CreateBookingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/public/tenants/{tenantSlug}/bookings", Handle)
            .RequireRateLimiting(RateLimitPolicies.PublicBooking)
            .WithTags("Public Bookings")
            .WithName("CreateBooking");
    }

    private static async Task<IResult> Handle(
        string tenantSlug,
        CreateBookingRequest request,
        ITenantSlugResolver tenantSlugResolver,
        ICommandHandler<CreateBookingCommand, BookingResponse> handler,
        CancellationToken cancellationToken)
    {
        Guid? tenantId = await tenantSlugResolver.ResolveTenantIdAsync(tenantSlug, cancellationToken);

        if (tenantId is null)
        {
            return TypedResults.NotFound();
        }

        BookingResponse response = await handler.Handle(
            new CreateBookingCommand(
                tenantId.Value,
                request.CustomerName,
                request.CustomerEmail,
                request.CustomerPhoneNumber,
                request.IdempotencyKey,
                request.ServiceId,
                request.StaffMemberId,
                request.ResourceId,
                request.StartsAtUtc,
                request.EndsAtUtc),
            cancellationToken);

        return TypedResults.Created(
            $"/api/public/tenants/{tenantSlug}/bookings/{response.PublicReference}",
            response);
    }
}
