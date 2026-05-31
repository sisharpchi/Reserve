using ReserveFlow.Modules.Tenants.Domain.TenantCategories;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories;

public interface ITenantCategoryRepository
{
    void Insert(TenantCategory category);

    Task<IReadOnlyList<TenantCategory>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
