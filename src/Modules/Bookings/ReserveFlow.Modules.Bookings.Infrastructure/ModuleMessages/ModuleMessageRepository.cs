using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Common.Infrastructure.Outbox;
using ReserveFlow.Modules.Bookings.Application.ModuleMessages;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;

namespace ReserveFlow.Modules.Bookings.Infrastructure.ModuleMessages;

internal sealed class ModuleMessageRepository(BookingsDbContext dbContext) : IModuleMessageRepository
{
    public async Task<PagedResult<ModuleMessageResponse>> GetByTenantIdAsync(
        Guid tenantId,
        PageRequest pageRequest,
        string? status,
        string? type,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        IQueryable<OutboxMessage> query = dbContext.OutboxMessages
            .AsNoTracking()
            .Where(message => message.TenantId == tenantId);

        query = ApplyStatusFilter(query, status);
        query = ApplyTypeFilter(query, type);

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        OutboxMessage[] messages = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .ToArrayAsync(cancellationToken);

        ModuleMessageResponse[] items = messages
            .Select(message => new ModuleMessageResponse(
                message.Id,
                tenantId,
                "Bookings",
                message.Type,
                message.OccurredOnUtc,
                message.ProcessedOnUtc,
                ResolveStatus(message),
                message.Error,
                message.RetryCount))
            .ToArray();

        return new PagedResult<ModuleMessageResponse>(items, totalCount);
    }

    private static string ResolveStatus(OutboxMessage message)
    {
        if (message.ProcessedOnUtc is not null)
        {
            return "Processed";
        }

        return string.IsNullOrWhiteSpace(message.Error) ? "Pending" : "Failed";
    }

    private static IQueryable<OutboxMessage> ApplyStatusFilter(
        IQueryable<OutboxMessage> query,
        string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return query;
        }

        return status.Trim().ToLowerInvariant() switch
        {
            "processed" => query.Where(message => message.ProcessedOnUtc != null),
            "pending" => query.Where(message =>
                message.ProcessedOnUtc == null &&
                (message.Error == null || message.Error == string.Empty)),
            "failed" => query.Where(message =>
                message.ProcessedOnUtc == null &&
                message.Error != null &&
                message.Error != string.Empty),
            _ => query
        };
    }

    private static IQueryable<OutboxMessage> ApplyTypeFilter(
        IQueryable<OutboxMessage> query,
        string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return query;
        }

        string typePattern = $"%{type.Trim()}%";

        return query.Where(message => EF.Functions.ILike(message.Type, typePattern));
    }

    private static IOrderedQueryable<OutboxMessage> ApplySorting(
        IQueryable<OutboxMessage> query,
        string? sortBy,
        string? sortDirection)
    {
        bool descending = IsDescending(sortDirection, defaultValue: true);

        return NormalizeSortKey(sortBy) switch
        {
            "type" => descending
                ? query.OrderByDescending(message => message.Type).ThenByDescending(message => message.OccurredOnUtc)
                : query.OrderBy(message => message.Type).ThenBy(message => message.OccurredOnUtc),
            "processedon" or "processedonutc" => descending
                ? query.OrderByDescending(message => message.ProcessedOnUtc).ThenByDescending(message => message.OccurredOnUtc)
                : query.OrderBy(message => message.ProcessedOnUtc).ThenBy(message => message.OccurredOnUtc),
            "retrycount" => descending
                ? query.OrderByDescending(message => message.RetryCount).ThenByDescending(message => message.OccurredOnUtc)
                : query.OrderBy(message => message.RetryCount).ThenBy(message => message.OccurredOnUtc),
            "status" => descending
                ? query.OrderByDescending(message => message.ProcessedOnUtc != null)
                    .ThenByDescending(message => message.Error != null && message.Error != string.Empty)
                    .ThenByDescending(message => message.OccurredOnUtc)
                : query.OrderBy(message => message.ProcessedOnUtc != null)
                    .ThenBy(message => message.Error != null && message.Error != string.Empty)
                    .ThenBy(message => message.OccurredOnUtc),
            _ => descending
                ? query.OrderByDescending(message => message.OccurredOnUtc)
                : query.OrderBy(message => message.OccurredOnUtc)
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
