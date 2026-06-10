using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Notifications.Application.Notifications.QueueNotification;

public sealed class QueueNotificationCommandHandler(
    INotificationRepository notificationRepository,
    INotificationsUnitOfWork unitOfWork) : ICommandHandler<QueueNotificationCommand, NotificationResponse>
{
    public async Task<NotificationResponse> Handle(
        QueueNotificationCommand command,
        CancellationToken cancellationToken = default)
    {
        NotificationMessage message = NotificationMessage.Queue(
            command.TenantId,
            command.Channel,
            command.Recipient,
            command.Subject,
            command.Body,
            command.DeliverAtUtc);

        notificationRepository.Insert(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NotificationResponse.FromMessage(message);
    }
}
