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

    public Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return dbContext.Tenants
            .FirstOrDefaultAsync(tenant => tenant.Id == tenantId, cancellationToken);
    }

    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return dbContext.Tenants
            .FirstOrDefaultAsync(tenant => tenant.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Tenant>> GetPublicAsync(
        Guid? categoryId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Tenant> query = dbContext.Tenants
            .Where(tenant => tenant.Status == TenantStatus.Active);

        if (categoryId is not null)
        {
            query = query.Where(tenant => tenant.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string searchPattern = $"%{search.Trim()}%";
            query = query.Where(tenant =>
                EF.Functions.ILike(tenant.Name, searchPattern) ||
                EF.Functions.ILike(tenant.Slug, searchPattern));
        }

        return await query
            .OrderBy(tenant => tenant.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return dbContext.Tenants
            .AnyAsync(tenant => tenant.Slug == slug, cancellationToken);
    }
}
