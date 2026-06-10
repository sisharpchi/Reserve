using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Notifications.Application.Notifications.CancelPendingNotifications;

public sealed class CancelPendingNotificationsCommandHandler(
    INotificationRepository notificationRepository,
    INotificationsUnitOfWork unitOfWork) : ICommandHandler<CancelPendingNotificationsCommand, int>
{
    public async Task<int> Handle(
        CancelPendingNotificationsCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.TenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id is required.");
        }

        string correlationKey = NormalizeRequired(command.CorrelationKey, "Notification correlation key");
        string reason = NormalizeRequired(command.Reason, "Notification cancellation reason");

        IReadOnlyList<NotificationMessage> messages = await notificationRepository.GetPendingByCorrelationKeyAsync(
            command.TenantId,
            correlationKey,
            cancellationToken);

        foreach (NotificationMessage message in messages)
        {
            message.Cancel(reason);
        }

        if (messages.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return messages.Count;
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }
}
