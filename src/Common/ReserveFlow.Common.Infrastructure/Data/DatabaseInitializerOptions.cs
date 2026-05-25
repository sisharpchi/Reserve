namespace ReserveFlow.Common.Infrastructure.Data;

internal sealed class DatabaseInitializerOptions
{
    public bool Enabled { get; init; }

    public string[] ModuleSchemas { get; init; } = [];
}
