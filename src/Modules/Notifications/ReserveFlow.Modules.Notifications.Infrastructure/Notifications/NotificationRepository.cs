using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Notifications.Application.Notifications;
using ReserveFlow.Modules.Notifications.Domain.Notifications;
using ReserveFlow.Modules.Notifications.Infrastructure.Database;

namespace ReserveFlow.Modules.Notifications.Infrastructure.Notifications;

internal sealed class NotificationRepository(NotificationsDbContext dbContext) : INotificationRepository
{
    public void Insert(NotificationMessage message)
    {
        dbContext.NotificationMessages.Add(message);
    }

    public async Task<IReadOnlyList<NotificationMessage>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.NotificationMessages
            .Where(message => message.TenantId == tenantId)
            .OrderByDescending(message => message.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PagedResult<NotificationMessage>> GetByTenantIdAsync(
        Guid tenantId,
        PageRequest pageRequest,
        string? status,
        string? channel,
        string? search,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        IQueryable<NotificationMessage> query = dbContext.NotificationMessages
            .Where(message => message.TenantId == tenantId);

        query = ApplyStatusFilter(query, status);
        query = ApplyChannelFilter(query, channel);
        query = ApplySearch(query, search);

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortDirection);

        NotificationMessage[] items = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<NotificationMessage>(items, totalCount);
    }

    public async Task<IReadOnlyList<NotificationMessage>> GetPendingByCorrelationKeyAsync(
        Guid tenantId,
        string correlationKey,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.NotificationMessages
            .Where(message =>
                message.TenantId == tenantId &&
                message.CorrelationKey == correlationKey &&
                message.Status == NotificationStatus.Pending)
            .OrderBy(message => message.DeliverAtUtc)
            .ThenBy(message => message.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    private static IQueryable<NotificationMessage> ApplyStatusFilter(
        IQueryable<NotificationMessage> query,
        string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return query;
        }

        return Enum.TryParse(status.Trim(), ignoreCase: true, out NotificationStatus notificationStatus)
            ? query.Where(message => message.Status == notificationStatus)
            : query;
    }

    private static IQueryable<NotificationMessage> ApplyChannelFilter(
        IQueryable<NotificationMessage> query,
        string? channel)
    {
        if (string.IsNullOrWhiteSpace(channel))
        {
            return query;
        }

        return Enum.TryParse(channel.Trim(), ignoreCase: true, out NotificationChannel notificationChannel)
            ? query.Where(message => message.Channel == notificationChannel)
            : query;
    }

    private static IQueryable<NotificationMessage> ApplySearch(
        IQueryable<NotificationMessage> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        string searchPattern = $"%{search.Trim()}%";

        return query.Where(message =>
            EF.Functions.ILike(message.Recipient, searchPattern) ||
            EF.Functions.ILike(message.Subject, searchPattern) ||
            (message.CorrelationKey != null && EF.Functions.ILike(message.CorrelationKey, searchPattern)));
    }

    private static IOrderedQueryable<NotificationMessage> ApplySorting(
        IQueryable<NotificationMessage> query,
        string? sortBy,
        string? sortDirection)
    {
        bool descending = IsDescending(sortDirection, defaultValue: true);

        return NormalizeSortKey(sortBy) switch
        {
            "channel" => descending
                ? query.OrderByDescending(message => message.Channel).ThenByDescending(message => message.CreatedAtUtc)
                : query.OrderBy(message => message.Channel).ThenBy(message => message.CreatedAtUtc),
            "status" => descending
                ? query.OrderByDescending(message => message.Status).ThenByDescending(message => message.CreatedAtUtc)
                : query.OrderBy(message => message.Status).ThenBy(message => message.CreatedAtUtc),
            "deliverat" or "deliveratutc" => descending
                ? query.OrderByDescending(message => message.DeliverAtUtc).ThenByDescending(message => message.CreatedAtUtc)
                : query.OrderBy(message => message.DeliverAtUtc).ThenBy(message => message.CreatedAtUtc),
            "sentat" or "sentatutc" => descending
                ? query.OrderByDescending(message => message.SentAtUtc).ThenByDescending(message => message.CreatedAtUtc)
                : query.OrderBy(message => message.SentAtUtc).ThenBy(message => message.CreatedAtUtc),
            _ => descending
                ? query.OrderByDescending(message => message.CreatedAtUtc)
                : query.OrderBy(message => message.CreatedAtUtc)
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
