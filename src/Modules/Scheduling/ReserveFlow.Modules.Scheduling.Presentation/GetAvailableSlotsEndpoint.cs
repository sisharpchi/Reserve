using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.RateLimiting;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Scheduling.Application.Availability;
using ReserveFlow.Modules.Scheduling.Application.Availability.GetAvailableSlots;

namespace ReserveFlow.Modules.Scheduling.Presentation;

internal sealed class GetAvailableSlotsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/availability", Handle)
            .RequireRateLimiting(RateLimitPolicies.PublicAvailability)
            .WithTags("Public Scheduling")
            .WithName("GetAvailableSlots");
    }

    private static async Task<Ok<IReadOnlyList<AvailableSlotResponse>>> Handle(
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        int durationMinutes,
        int? stepMinutes,
        IQueryHandler<GetAvailableSlotsQuery, IReadOnlyList<AvailableSlotResponse>> handler,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AvailableSlotResponse> response = await handler.Handle(
            new GetAvailableSlotsQuery(
                startsAtUtc,
                endsAtUtc,
                durationMinutes,
                stepMinutes ?? durationMinutes),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
