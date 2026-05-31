namespace ReserveFlow.Modules.Notifications.Application.Notifications;

public interface INotificationSender
{
    Task SendAsync(NotificationDeliveryMessage message, CancellationToken cancellationToken = default);
}
