using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Notifications.Application.Notifications.QueueNotification;

public sealed record QueueNotificationCommand(
    Guid TenantId,
    NotificationChannel Channel,
    string Recipient,
    string Subject,
    string Body,
    DateTime? DeliverAtUtc = null) : ICommand<NotificationResponse>;
