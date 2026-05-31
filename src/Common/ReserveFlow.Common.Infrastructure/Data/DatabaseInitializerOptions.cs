namespace ReserveFlow.Common.Infrastructure.Data;

internal sealed class DatabaseInitializerOptions
{
    public bool Enabled { get; init; }

    public bool CreateDatabaseIfMissing { get; init; }

    public string MaintenanceDatabase { get; init; } = "postgres";

    public string[] ModuleSchemas { get; init; } = [];
}
