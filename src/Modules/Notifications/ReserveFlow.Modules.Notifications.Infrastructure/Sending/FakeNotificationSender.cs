using ReserveFlow.Modules.Notifications.Application.Notifications;

namespace ReserveFlow.Modules.Notifications.Infrastructure.Sending;

public sealed class FakeNotificationSender : INotificationSender
{
    public Task SendAsync(
        NotificationDeliveryMessage message,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
