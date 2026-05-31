using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Notifications.Application.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    Guid TenantId,
    string Channel,
    string Recipient,
    string Subject,
    string Body,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? SentAtUtc,
    string? Error)
{
    public static NotificationResponse FromMessage(NotificationMessage message)
    {
        return new NotificationResponse(
            message.Id,
            message.TenantId,
            message.Channel.ToString(),
            message.Recipient,
            message.Subject,
            message.Body,
            message.Status.ToString(),
            message.CreatedAtUtc,
            message.SentAtUtc,
            message.Error);
    }
}
