using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Notifications.Application.Notifications;

public interface INotificationRepository
{
    void Insert(NotificationMessage message);
}
