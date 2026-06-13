namespace ReserveFlow.Common.Infrastructure.Data;

internal sealed class DatabaseInitializerOptions
{
    public bool Enabled { get; init; }

    public bool CreateDatabaseIfMissing { get; init; }

    public string MaintenanceDatabase { get; init; } = "postgres";

    public int StartupRetryAttempts { get; init; } = 15;

    public int StartupRetryDelayMilliseconds { get; init; } = 1_000;

    public string[] ModuleSchemas { get; init; } = [];

    public int GetSafeStartupRetryAttempts()
    {
        return Math.Clamp(StartupRetryAttempts, 1, 60);
    }

    public TimeSpan GetSafeStartupRetryDelay()
    {
        int delayMilliseconds = Math.Clamp(StartupRetryDelayMilliseconds, 100, 30_000);

        return TimeSpan.FromMilliseconds(delayMilliseconds);
    }
}
