namespace ReserveFlow.Modules.Notifications.Application.Notifications;

public sealed record NotificationDeliveryMessage(
    Guid NotificationId,
    string Channel,
    string Recipient,
    string Subject,
    string Body);
