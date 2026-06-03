using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.RateLimiting;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Scheduling.Application.Availability;
using ReserveFlow.Modules.Scheduling.Application.Availability.GetTenantAvailableSlots;

namespace ReserveFlow.Modules.Scheduling.Presentation;

internal sealed class GetTenantAvailableSlotsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/tenants/{tenantSlug}/availability", Handle)
            .RequireRateLimiting(RateLimitPolicies.PublicAvailability)
            .WithTags("Public Scheduling")
            .WithName("GetTenantAvailableSlots");
    }

    private static async Task<Results<Ok<IReadOnlyList<AvailableSlotResponse>>, NotFound>> Handle(
        string tenantSlug,
        DateOnly date,
        Guid? staffMemberId,
        Guid? resourceId,
        int durationMinutes,
        int? stepMinutes,
        ITenantSlugResolver tenantSlugResolver,
        IQueryHandler<GetTenantAvailableSlotsQuery, IReadOnlyList<AvailableSlotResponse>> handler,
        CancellationToken cancellationToken)
    {
        Guid? tenantId = await tenantSlugResolver.ResolveTenantIdAsync(tenantSlug, cancellationToken);

        if (tenantId is null)
        {
            return TypedResults.NotFound();
        }

        IReadOnlyList<AvailableSlotResponse> response = await handler.Handle(
            new GetTenantAvailableSlotsQuery(
                tenantId.Value,
                date,
                staffMemberId,
                resourceId,
                durationMinutes,
                stepMinutes),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
