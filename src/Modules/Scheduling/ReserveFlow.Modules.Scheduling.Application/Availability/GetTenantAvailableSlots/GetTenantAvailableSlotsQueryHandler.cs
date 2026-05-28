using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours;
using ReserveFlow.Modules.Scheduling.Domain.Availability;
using ReserveFlow.Modules.Scheduling.Domain.WorkingHours;

namespace ReserveFlow.Modules.Scheduling.Application.Availability.GetTenantAvailableSlots;

public sealed class GetTenantAvailableSlotsQueryHandler(IWorkingHourRepository workingHourRepository)
    : IQueryHandler<GetTenantAvailableSlotsQuery, IReadOnlyList<AvailableSlotResponse>>
{
    public async Task<IReadOnlyList<AvailableSlotResponse>> Handle(
        GetTenantAvailableSlotsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.DurationMinutes <= 0)
        {
            throw new InvalidOperationException("Duration minutes must be greater than zero.");
        }

        int stepMinutes = query.StepMinutes ?? query.DurationMinutes;

        if (stepMinutes <= 0)
        {
            throw new InvalidOperationException("Step minutes must be greater than zero.");
        }

        IReadOnlyList<WorkingHour> workingHours = await workingHourRepository.GetByTargetAndDayAsync(
            query.TenantId,
            query.StaffMemberId,
            query.ResourceId,
            query.Date.DayOfWeek,
            cancellationToken);

        IReadOnlyList<AvailabilityWindow> windows = BuildAvailabilityWindows(
            workingHours,
            query.Date,
            query.StaffMemberId,
            query.ResourceId);

        IReadOnlyList<AvailableSlot> slots = AvailabilityEngine.GenerateSlots(
            windows,
            TimeSpan.FromMinutes(query.DurationMinutes),
            TimeSpan.FromMinutes(stepMinutes));

        return slots
            .Select(AvailableSlotResponse.FromSlot)
            .ToArray();
    }

    private static IReadOnlyList<AvailabilityWindow> BuildAvailabilityWindows(
        IReadOnlyList<WorkingHour> workingHours,
        DateOnly date,
        Guid? staffMemberId,
        Guid? resourceId)
    {
        if (staffMemberId is not null && resourceId is not null)
        {
            AvailabilityWindow[] staffWindows = workingHours
                .Where(workingHour => workingHour.StaffMemberId == staffMemberId.Value)
                .Select(workingHour => workingHour.ToAvailabilityWindow(date))
                .ToArray();

            AvailabilityWindow[] resourceWindows = workingHours
                .Where(workingHour => workingHour.ResourceId == resourceId.Value)
                .Select(workingHour => workingHour.ToAvailabilityWindow(date))
                .ToArray();

            return IntersectWindows(staffWindows, resourceWindows);
        }

        return workingHours
            .Select(workingHour => workingHour.ToAvailabilityWindow(date))
            .ToArray();
    }

    private static List<AvailabilityWindow> IntersectWindows(
        IReadOnlyList<AvailabilityWindow> first,
        IReadOnlyList<AvailabilityWindow> second)
    {
        var intersections = new List<AvailabilityWindow>();

        foreach (AvailabilityWindow left in first)
        {
            foreach (AvailabilityWindow right in second)
            {
                DateTimeOffset startsAtUtc = left.StartsAtUtc > right.StartsAtUtc
                    ? left.StartsAtUtc
                    : right.StartsAtUtc;

                DateTimeOffset endsAtUtc = left.EndsAtUtc < right.EndsAtUtc
                    ? left.EndsAtUtc
                    : right.EndsAtUtc;

                if (endsAtUtc > startsAtUtc)
                {
                    intersections.Add(new AvailabilityWindow(startsAtUtc, endsAtUtc));
                }
            }
        }

        return intersections;
    }
}
