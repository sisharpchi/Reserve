using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Application.Pagination;
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

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Tenants
            .OrderBy(tenant => tenant.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PagedResult<Tenant>> GetAllAsync(
        PageRequest pageRequest,
        Guid? categoryId,
        string? search,
        string? status,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Tenant> query = dbContext.Tenants;

        query = ApplyCategoryFilter(query, categoryId);
        query = ApplyPlatformSearch(query, search);
        query = ApplyStatusFilter(query, status);

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplyPlatformSorting(query, sortBy, sortDirection);

        Tenant[] items = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<Tenant>(items, totalCount);
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

    private static IQueryable<Tenant> ApplyCategoryFilter(IQueryable<Tenant> query, Guid? categoryId)
    {
        return categoryId.HasValue
            ? query.Where(tenant => tenant.CategoryId == categoryId.Value)
            : query;
    }

    private static IQueryable<Tenant> ApplyPlatformSearch(IQueryable<Tenant> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        string searchPattern = $"%{search.Trim()}%";

        return query.Where(tenant =>
            EF.Functions.ILike(tenant.Name, searchPattern) ||
            EF.Functions.ILike(tenant.Slug, searchPattern) ||
            EF.Functions.ILike(tenant.TimeZoneId, searchPattern));
    }

    private static IQueryable<Tenant> ApplyStatusFilter(IQueryable<Tenant> query, string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return query;
        }

        return Enum.TryParse(status.Trim(), ignoreCase: true, out TenantStatus tenantStatus)
            ? query.Where(tenant => tenant.Status == tenantStatus)
            : query;
    }

    private static IOrderedQueryable<Tenant> ApplyPlatformSorting(
        IQueryable<Tenant> query,
        string? sortBy,
        string? sortDirection)
    {
        bool descending = IsDescending(sortDirection);

        return NormalizeSortKey(sortBy) switch
        {
            "slug" => descending
                ? query.OrderByDescending(tenant => tenant.Slug)
                : query.OrderBy(tenant => tenant.Slug),
            "status" => descending
                ? query.OrderByDescending(tenant => tenant.Status).ThenBy(tenant => tenant.Name)
                : query.OrderBy(tenant => tenant.Status).ThenBy(tenant => tenant.Name),
            "categoryid" => descending
                ? query.OrderByDescending(tenant => tenant.CategoryId).ThenBy(tenant => tenant.Name)
                : query.OrderBy(tenant => tenant.CategoryId).ThenBy(tenant => tenant.Name),
            "createdat" or "createdatutc" => descending
                ? query.OrderByDescending(tenant => tenant.CreatedAtUtc).ThenBy(tenant => tenant.Name)
                : query.OrderBy(tenant => tenant.CreatedAtUtc).ThenBy(tenant => tenant.Name),
            _ => descending
                ? query.OrderByDescending(tenant => tenant.Name)
                : query.OrderBy(tenant => tenant.Name)
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
