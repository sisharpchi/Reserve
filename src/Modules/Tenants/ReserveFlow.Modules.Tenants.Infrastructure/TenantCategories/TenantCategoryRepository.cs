using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Tenants.Application.TenantCategories;
using ReserveFlow.Modules.Tenants.Domain.TenantCategories;
using ReserveFlow.Modules.Tenants.Infrastructure.Database;

namespace ReserveFlow.Modules.Tenants.Infrastructure.TenantCategories;

internal sealed class TenantCategoryRepository(TenantsDbContext dbContext) : ITenantCategoryRepository
{
    public void Insert(TenantCategory category)
    {
        dbContext.TenantCategories.Add(category);
    }

    public async Task<IReadOnlyList<TenantCategory>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantCategories
            .Where(category => category.IsActive)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return dbContext.TenantCategories
            .AnyAsync(category => category.Slug == slug, cancellationToken);
    }
}
