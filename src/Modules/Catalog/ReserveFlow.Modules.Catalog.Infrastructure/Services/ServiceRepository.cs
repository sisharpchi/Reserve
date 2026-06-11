using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Domain.Services;
using ReserveFlow.Modules.Catalog.Infrastructure.Database;

namespace ReserveFlow.Modules.Catalog.Infrastructure.Services;

internal sealed class ServiceRepository(CatalogDbContext dbContext) : IServiceRepository
{
    public void Insert(Service service)
    {
        dbContext.Services.Add(service);
    }

    public async Task<Service?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Services.FirstOrDefaultAsync(
            service => service.Id == serviceId,
            cancellationToken);
    }

    public async Task<Service?> GetActiveByIdAsync(
        Guid tenantId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Services.FirstOrDefaultAsync(
            service => service.TenantId == tenantId &&
                       service.Id == serviceId &&
                       service.IsActive,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Services
            .Where(service => service.TenantId == tenantId)
            .OrderBy(service => service.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PagedResult<Service>> GetByTenantIdAsync(
        Guid tenantId,
        PageRequest pageRequest,
        string? search,
        bool? isActive,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Service> query = dbContext.Services
            .Where(service => service.TenantId == tenantId);

        query = ApplySearch(query, search);
        query = ApplyActiveFilter(query, isActive);

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        Service[] items = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<Service>(items, totalCount);
    }

    public async Task<IReadOnlyList<Service>> GetActiveByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Services
            .Where(service => service.TenantId == tenantId && service.IsActive)
            .OrderBy(service => service.Name)
            .ToArrayAsync(cancellationToken);
    }

    private static IQueryable<Service> ApplySearch(IQueryable<Service> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        string searchPattern = $"%{search.Trim()}%";

        return query.Where(service => EF.Functions.ILike(service.Name, searchPattern));
    }

    private static IQueryable<Service> ApplyActiveFilter(IQueryable<Service> query, bool? isActive)
    {
        return isActive.HasValue
            ? query.Where(service => service.IsActive == isActive.Value)
            : query;
    }

    private static IOrderedQueryable<Service> ApplySorting(
        IQueryable<Service> query,
        string? sortBy,
        string? sortDirection)
    {
        bool descending = IsDescending(sortDirection);

        return NormalizeSortKey(sortBy) switch
        {
            "duration" or "durationminutes" => descending
                ? query.OrderByDescending(service => service.DurationMinutes).ThenBy(service => service.Name)
                : query.OrderBy(service => service.DurationMinutes).ThenBy(service => service.Name),
            "price" => descending
                ? query.OrderByDescending(service => service.Price).ThenBy(service => service.Name)
                : query.OrderBy(service => service.Price).ThenBy(service => service.Name),
            "createdat" or "createdatutc" => descending
                ? query.OrderByDescending(service => service.CreatedAtUtc).ThenBy(service => service.Name)
                : query.OrderBy(service => service.CreatedAtUtc).ThenBy(service => service.Name),
            _ => descending
                ? query.OrderByDescending(service => service.Name)
                : query.OrderBy(service => service.Name)
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
