namespace ReserveFlow.Modules.Tenants.Presentation;

public sealed record UpdateTenantRequest(
    string Name,
    string Slug,
    string TimeZoneId,
    Guid? CategoryId);
