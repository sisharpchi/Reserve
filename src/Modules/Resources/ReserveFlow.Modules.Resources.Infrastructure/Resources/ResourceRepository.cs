using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Domain.Resources;
using ReserveFlow.Modules.Resources.Infrastructure.Database;

namespace ReserveFlow.Modules.Resources.Infrastructure.Resources;

internal sealed class ResourceRepository(ResourcesDbContext dbContext) : IResourceRepository
{
    public void Insert(Resource resource)
    {
        dbContext.Resources.Add(resource);
    }

    public async Task<Resource?> GetByIdAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Resources.FirstOrDefaultAsync(
            resource => resource.Id == resourceId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Resource>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Resources
            .Where(resource => resource.TenantId == tenantId)
            .OrderBy(resource => resource.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PagedResult<Resource>> GetByTenantIdAsync(
        Guid tenantId,
        PageRequest pageRequest,
        string? search,
        bool? isActive,
        string? resourceType,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Resource> query = dbContext.Resources
            .Where(resource => resource.TenantId == tenantId);

        query = ApplySearch(query, search);
        query = ApplyActiveFilter(query, isActive);
        query = ApplyResourceTypeFilter(query, resourceType);

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        Resource[] items = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<Resource>(items, totalCount);
    }

    public async Task<IReadOnlyList<Resource>> GetActiveByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Resources
            .Where(resource => resource.TenantId == tenantId && resource.IsActive)
            .OrderBy(resource => resource.Name)
            .ToArrayAsync(cancellationToken);
    }

    private static IQueryable<Resource> ApplySearch(IQueryable<Resource> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        string searchPattern = $"%{search.Trim()}%";

        return query.Where(resource => EF.Functions.ILike(resource.Name, searchPattern));
    }

    private static IQueryable<Resource> ApplyActiveFilter(IQueryable<Resource> query, bool? isActive)
    {
        return isActive.HasValue
            ? query.Where(resource => resource.IsActive == isActive.Value)
            : query;
    }

    private static IQueryable<Resource> ApplyResourceTypeFilter(IQueryable<Resource> query, string? resourceType)
    {
        if (string.IsNullOrWhiteSpace(resourceType))
        {
            return query;
        }

        string normalizedResourceType = resourceType.Trim().ToLowerInvariant();

        return query.Where(resource => resource.ResourceType == normalizedResourceType);
    }

    private static IOrderedQueryable<Resource> ApplySorting(
        IQueryable<Resource> query,
        string? sortBy,
        string? sortDirection)
    {
        bool descending = IsDescending(sortDirection);

        return NormalizeSortKey(sortBy) switch
        {
            "resourcetype" => descending
                ? query.OrderByDescending(resource => resource.ResourceType).ThenBy(resource => resource.Name)
                : query.OrderBy(resource => resource.ResourceType).ThenBy(resource => resource.Name),
            "capacity" => descending
                ? query.OrderByDescending(resource => resource.Capacity).ThenBy(resource => resource.Name)
                : query.OrderBy(resource => resource.Capacity).ThenBy(resource => resource.Name),
            "createdat" or "createdatutc" => descending
                ? query.OrderByDescending(resource => resource.CreatedAtUtc).ThenBy(resource => resource.Name)
                : query.OrderBy(resource => resource.CreatedAtUtc).ThenBy(resource => resource.Name),
            _ => descending
                ? query.OrderByDescending(resource => resource.Name)
                : query.OrderBy(resource => resource.Name)
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
