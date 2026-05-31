namespace ReserveFlow.Modules.Scheduling.Domain.Availability;

public sealed record AvailabilityWindow
{
    public AvailabilityWindow(DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc)
    {
        if (endsAtUtc <= startsAtUtc)
        {
            throw new InvalidOperationException("Availability window end must be after start.");
        }

        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
    }

    public DateTimeOffset StartsAtUtc { get; }

    public DateTimeOffset EndsAtUtc { get; }
}
