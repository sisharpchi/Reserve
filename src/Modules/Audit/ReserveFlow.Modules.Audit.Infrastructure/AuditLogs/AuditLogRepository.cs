using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Audit.Application.AuditLogs;
using ReserveFlow.Modules.Audit.Domain.AuditLogs;
using ReserveFlow.Modules.Audit.Infrastructure.Database;

namespace ReserveFlow.Modules.Audit.Infrastructure.AuditLogs;

internal sealed class AuditLogRepository(AuditDbContext dbContext) : IAuditLogRepository
{
    public void Insert(AuditLog log)
    {
        dbContext.AuditLogs.Add(log);
    }

    public async Task<PagedResult<AuditLog>> GetRecentAsync(
        PageRequest pageRequest,
        Guid? tenantId,
        string? action,
        string? entityName,
        DateTime? fromUtc,
        DateTime? toUtc,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AuditLog> query = dbContext.AuditLogs;

        query = ApplyTenantFilter(query, tenantId);
        query = ApplyActionFilter(query, action);
        query = ApplyEntityNameFilter(query, entityName);
        query = ApplyDateRangeFilter(query, fromUtc, toUtc);

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        AuditLog[] items = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<AuditLog>(items, totalCount);
    }

    private static IQueryable<AuditLog> ApplyTenantFilter(IQueryable<AuditLog> query, Guid? tenantId)
    {
        return tenantId.HasValue
            ? query.Where(log => log.TenantId == tenantId.Value)
            : query;
    }

    private static IQueryable<AuditLog> ApplyActionFilter(IQueryable<AuditLog> query, string? action)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            return query;
        }

        string actionPattern = $"%{action.Trim()}%";

        return query.Where(log => EF.Functions.ILike(log.Action, actionPattern));
    }

    private static IQueryable<AuditLog> ApplyEntityNameFilter(IQueryable<AuditLog> query, string? entityName)
    {
        if (string.IsNullOrWhiteSpace(entityName))
        {
            return query;
        }

        string entityNamePattern = $"%{entityName.Trim()}%";

        return query.Where(log => EF.Functions.ILike(log.EntityName, entityNamePattern));
    }

    private static IQueryable<AuditLog> ApplyDateRangeFilter(
        IQueryable<AuditLog> query,
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        if (fromUtc.HasValue)
        {
            query = query.Where(log => log.OccurredOnUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(log => log.OccurredOnUtc <= toUtc.Value);
        }

        return query;
    }

    private static IOrderedQueryable<AuditLog> ApplySorting(
        IQueryable<AuditLog> query,
        string? sortBy,
        string? sortDirection)
    {
        bool descending = IsDescending(sortDirection, defaultValue: true);

        return NormalizeSortKey(sortBy) switch
        {
            "tenantid" => descending
                ? query.OrderByDescending(log => log.TenantId).ThenByDescending(log => log.OccurredOnUtc)
                : query.OrderBy(log => log.TenantId).ThenBy(log => log.OccurredOnUtc),
            "action" => descending
                ? query.OrderByDescending(log => log.Action).ThenByDescending(log => log.OccurredOnUtc)
                : query.OrderBy(log => log.Action).ThenBy(log => log.OccurredOnUtc),
            "entityname" => descending
                ? query.OrderByDescending(log => log.EntityName).ThenByDescending(log => log.OccurredOnUtc)
                : query.OrderBy(log => log.EntityName).ThenBy(log => log.OccurredOnUtc),
            _ => descending
                ? query.OrderByDescending(log => log.OccurredOnUtc)
                : query.OrderBy(log => log.OccurredOnUtc)
        };
    }

    private static bool IsDescending(string? sortDirection, bool defaultValue = false)
    {
        if (string.IsNullOrWhiteSpace(sortDirection))
        {
            return defaultValue;
        }

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
