using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Application.Pagination;
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

    public Task<TenantCategory?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return dbContext.TenantCategories
            .FirstOrDefaultAsync(category => category.Id == categoryId, cancellationToken);
    }

    public async Task<IReadOnlyList<TenantCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantCategories
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PagedResult<TenantCategory>> GetAllAsync(
        PageRequest pageRequest,
        string? search,
        bool? isActive,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TenantCategory> query = dbContext.TenantCategories;

        query = ApplySearch(query, search);
        query = ApplyActiveFilter(query, isActive);

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        TenantCategory[] items = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<TenantCategory>(items, totalCount);
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

    private static IQueryable<TenantCategory> ApplySearch(IQueryable<TenantCategory> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        string searchPattern = $"%{search.Trim()}%";

        return query.Where(category =>
            EF.Functions.ILike(category.Name, searchPattern) ||
            EF.Functions.ILike(category.Slug, searchPattern));
    }

    private static IQueryable<TenantCategory> ApplyActiveFilter(
        IQueryable<TenantCategory> query,
        bool? isActive)
    {
        return isActive.HasValue
            ? query.Where(category => category.IsActive == isActive.Value)
            : query;
    }

    private static IOrderedQueryable<TenantCategory> ApplySorting(
        IQueryable<TenantCategory> query,
        string? sortBy,
        string? sortDirection)
    {
        bool descending = IsDescending(sortDirection);

        return NormalizeSortKey(sortBy) switch
        {
            "name" => descending
                ? query.OrderByDescending(category => category.Name)
                : query.OrderBy(category => category.Name),
            "slug" => descending
                ? query.OrderByDescending(category => category.Slug)
                : query.OrderBy(category => category.Slug),
            "createdat" or "createdatutc" => descending
                ? query.OrderByDescending(category => category.CreatedAtUtc).ThenBy(category => category.Name)
                : query.OrderBy(category => category.CreatedAtUtc).ThenBy(category => category.Name),
            _ => descending
                ? query.OrderByDescending(category => category.SortOrder).ThenByDescending(category => category.Name)
                : query.OrderBy(category => category.SortOrder).ThenBy(category => category.Name)
        };
    }

    private static bool IsDescending(string? sortDirection)
    {
        return string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sortDirection, "descending", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeSortKey(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy)
            ? string.Empty
            : sortBy.Trim().Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
    }
}
