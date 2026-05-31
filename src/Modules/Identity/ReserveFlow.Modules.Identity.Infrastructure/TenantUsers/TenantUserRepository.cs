using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Identity.Application.TenantUsers;
using ReserveFlow.Modules.Identity.Domain.TenantUsers;
using ReserveFlow.Modules.Identity.Infrastructure.Database;

namespace ReserveFlow.Modules.Identity.Infrastructure.TenantUsers;

internal sealed class TenantUserRepository(IdentityDbContext dbContext) : ITenantUserRepository
{
    public void Insert(TenantUser tenantUser)
    {
        dbContext.TenantUsers.Add(tenantUser);
    }

    public async Task<TenantUser?> GetByTenantAndUserIdAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantUsers.FirstOrDefaultAsync(
            tenantUser => tenantUser.TenantId == tenantId && tenantUser.UserId == userId,
            cancellationToken);
    }
}
