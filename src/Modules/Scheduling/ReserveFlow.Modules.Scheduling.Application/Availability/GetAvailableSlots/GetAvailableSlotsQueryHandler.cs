using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Scheduling.Domain.Availability;

namespace ReserveFlow.Modules.Scheduling.Application.Availability.GetAvailableSlots;

public sealed class GetAvailableSlotsQueryHandler
    : IQueryHandler<GetAvailableSlotsQuery, IReadOnlyList<AvailableSlotResponse>>
{
    public Task<IReadOnlyList<AvailableSlotResponse>> Handle(
        GetAvailableSlotsQuery query,
        CancellationToken cancellationToken = default)
    {
        var workingWindow = new AvailabilityWindow(query.StartsAtUtc, query.EndsAtUtc);
        IReadOnlyList<AvailableSlot> slots = AvailabilityEngine.GenerateSlots(
            [workingWindow],
            TimeSpan.FromMinutes(query.DurationMinutes),
            TimeSpan.FromMinutes(query.StepMinutes));

        IReadOnlyList<AvailableSlotResponse> response = slots
            .Select(AvailableSlotResponse.FromSlot)
            .ToArray();

        return Task.FromResult(response);
    }
}
