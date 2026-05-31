using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Scheduling.Application.UnavailablePeriods;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours;
using ReserveFlow.Modules.Scheduling.Domain.Availability;
using ReserveFlow.Modules.Scheduling.Domain.UnavailablePeriods;
using ReserveFlow.Modules.Scheduling.Domain.WorkingHours;

namespace ReserveFlow.Modules.Scheduling.Application.Availability.GetTenantAvailableSlots;

public sealed class GetTenantAvailableSlotsQueryHandler(
    IWorkingHourRepository workingHourRepository,
    IUnavailablePeriodRepository unavailablePeriodRepository)
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

        DateTimeOffset rangeStartsAtUtc = new(query.Date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        DateTimeOffset rangeEndsAtUtc = new(query.Date.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        IReadOnlyList<UnavailablePeriod> unavailablePeriods =
            await unavailablePeriodRepository.GetByTargetAndRangeAsync(
                query.TenantId,
                query.StaffMemberId,
                query.ResourceId,
                rangeStartsAtUtc,
                rangeEndsAtUtc,
                cancellationToken);

        windows = SubtractUnavailablePeriods(windows, unavailablePeriods);

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

    private static List<AvailabilityWindow> SubtractUnavailablePeriods(
        IReadOnlyList<AvailabilityWindow> windows,
        IReadOnlyList<UnavailablePeriod> unavailablePeriods)
    {
        List<AvailabilityWindow> availableWindows = windows.ToList();

        foreach (UnavailablePeriod unavailablePeriod in unavailablePeriods)
        {
            var nextWindows = new List<AvailabilityWindow>();

            foreach (AvailabilityWindow window in availableWindows)
            {
                if (unavailablePeriod.EndsAtUtc <= window.StartsAtUtc ||
                    unavailablePeriod.StartsAtUtc >= window.EndsAtUtc)
                {
                    nextWindows.Add(window);
                    continue;
                }

                DateTimeOffset leftEnd = unavailablePeriod.StartsAtUtc < window.EndsAtUtc
                    ? unavailablePeriod.StartsAtUtc
                    : window.EndsAtUtc;

                if (leftEnd > window.StartsAtUtc)
                {
                    nextWindows.Add(new AvailabilityWindow(window.StartsAtUtc, leftEnd));
                }

                DateTimeOffset rightStart = unavailablePeriod.EndsAtUtc > window.StartsAtUtc
                    ? unavailablePeriod.EndsAtUtc
                    : window.StartsAtUtc;

                if (window.EndsAtUtc > rightStart)
                {
                    nextWindows.Add(new AvailabilityWindow(rightStart, window.EndsAtUtc));
                }
            }

            availableWindows = nextWindows;
        }

        return availableWindows;
    }
}
