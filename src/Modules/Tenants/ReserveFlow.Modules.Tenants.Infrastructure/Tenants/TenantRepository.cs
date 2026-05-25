using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Domain.Tenants;
using ReserveFlow.Modules.Tenants.Infrastructure.Database;

namespace ReserveFlow.Modules.Tenants.Infrastructure.Tenants;

internal sealed class TenantRepository(TenantsDbContext dbContext) : ITenantRepository
{
    public void Insert(Tenant tenant)
    {
        dbContext.Tenants.Add(tenant);
    }

    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return dbContext.Tenants
            .FirstOrDefaultAsync(tenant => tenant.Slug == slug, cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return dbContext.Tenants
            .AnyAsync(tenant => tenant.Slug == slug, cancellationToken);
    }
}
