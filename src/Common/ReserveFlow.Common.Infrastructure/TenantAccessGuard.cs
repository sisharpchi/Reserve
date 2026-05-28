using ReserveFlow.Common.Application.Abstractions;

namespace ReserveFlow.Common.Infrastructure;

public sealed class TenantAccessGuard(ITenantContext tenantContext) : ITenantAccessGuard
{
    public bool CanAccessTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            return false;
        }

        if (tenantContext.IsPlatformScope)
        {
            return true;
        }

        return tenantContext.TenantId == tenantId;
    }
}
