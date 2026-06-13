namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetPlatformUsage;

public sealed record PlatformUsageResponse(
    int TotalTenants,
    int PendingTenants,
    int ActiveTenants,
    int SuspendedTenants,
    int DeletedTenants,
    int TotalCategories,
    int ActiveCategories,
    DateTimeOffset GeneratedAtUtc);
