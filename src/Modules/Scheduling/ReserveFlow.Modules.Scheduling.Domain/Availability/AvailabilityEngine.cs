namespace ReserveFlow.Modules.Scheduling.Domain.Availability;

public static class AvailabilityEngine
{
    public static IReadOnlyList<AvailableSlot> GenerateSlots(
        IReadOnlyCollection<AvailabilityWindow> workingWindows,
        TimeSpan serviceDuration,
        TimeSpan step)
    {
        if (serviceDuration <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("Service duration must be greater than zero.");
        }

        if (step <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("Slot step must be greater than zero.");
        }

        var slots = new List<AvailableSlot>();

        foreach (AvailabilityWindow window in workingWindows.OrderBy(window => window.StartsAtUtc))
        {
            DateTimeOffset startsAtUtc = window.StartsAtUtc;

            while (startsAtUtc + serviceDuration <= window.EndsAtUtc)
            {
                slots.Add(new AvailableSlot(startsAtUtc, startsAtUtc + serviceDuration));
                startsAtUtc += step;
            }
        }

        return slots;
    }
}
