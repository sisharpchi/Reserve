namespace ReserveFlow.Common.Application.Abstractions;

public interface ITenantAccessGuard
{
    bool CanAccessTenant(Guid tenantId);
}
