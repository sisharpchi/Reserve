using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Notifications.Application.Notifications;

public interface INotificationRepository
{
    void Insert(NotificationMessage message);

    Task<IReadOnlyList<NotificationMessage>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<NotificationMessage>> GetByTenantIdAsync(
        Guid tenantId,
        PageRequest pageRequest,
        string? status,
        string? channel,
        string? search,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationMessage>> GetPendingByCorrelationKeyAsync(
        Guid tenantId,
        string correlationKey,
        CancellationToken cancellationToken = default);
}
