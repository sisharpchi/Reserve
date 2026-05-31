namespace ReserveFlow.Modules.Catalog.Presentation;

internal sealed record CreateServiceRequest(
    Guid TenantId,
    string Name,
    int DurationMinutes,
    decimal? Price,
    string? Currency);
