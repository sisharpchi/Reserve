using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Notifications.Application.Notifications;

public interface INotificationRepository
{
    void Insert(NotificationMessage message);

    Task<IReadOnlyList<NotificationMessage>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
