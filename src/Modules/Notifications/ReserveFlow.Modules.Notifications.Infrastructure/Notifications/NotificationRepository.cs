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
}
