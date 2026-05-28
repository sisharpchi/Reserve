namespace ReserveFlow.Modules.Catalog.Presentation;

internal sealed record UpdateServiceRequest(
    string Name,
    int DurationMinutes,
    decimal? Price,
    string? Currency);
