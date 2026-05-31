using ReserveFlow.Modules.Identity.Domain.TenantUsers;

namespace ReserveFlow.Modules.Identity.Application.TenantUsers;

public interface ITenantUserRepository
{
    void Insert(TenantUser tenantUser);

    Task<TenantUser?> GetByTenantAndUserIdAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
