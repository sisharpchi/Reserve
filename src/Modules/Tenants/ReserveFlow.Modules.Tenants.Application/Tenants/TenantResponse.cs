using ReserveFlow.Modules.Tenants.Domain.Tenants;

namespace ReserveFlow.Modules.Tenants.Application.Tenants;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Slug,
    string TimeZoneId,
    string Status)
{
    public static TenantResponse FromTenant(Tenant tenant)
    {
        return new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.TimeZoneId,
            tenant.Status.ToString());
    }
}
