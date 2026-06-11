namespace ReserveFlow.Modules.Bookings.Infrastructure.Outbox;

internal sealed class BookingsOutboxOptions
{
    public bool Enabled { get; init; } = true;

    public int BatchSize { get; init; } = 25;

    public int PollingIntervalSeconds { get; init; } = 5;

    public int GetSafeBatchSize()
    {
        return BatchSize <= 0 ? 25 : BatchSize;
    }

    public TimeSpan GetSafePollingInterval()
    {
        return TimeSpan.FromSeconds(PollingIntervalSeconds <= 0 ? 5 : PollingIntervalSeconds);
    }
}
