namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed record CreateTenantRequest(
    string Name,
    string Slug,
    string TimeZoneId);
