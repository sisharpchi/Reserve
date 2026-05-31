namespace ReserveFlow.Modules.Identity.Application.CurrentUser;

public sealed record CurrentTenantResponse(
    Guid? TenantId,
    string? TenantSlug,
    string? TimeZoneId,
    bool IsPlatformScope);
