using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Tenants.Domain.TenantCategories;

namespace ReserveFlow.Modules.Tenants.Application.TenantCategories;

public interface ITenantCategoryRepository
{
    void Insert(TenantCategory category);

    Task<TenantCategory?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantCategory>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<TenantCategory>> GetAllAsync(
        PageRequest pageRequest,
        string? search,
        bool? isActive,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantCategory>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
