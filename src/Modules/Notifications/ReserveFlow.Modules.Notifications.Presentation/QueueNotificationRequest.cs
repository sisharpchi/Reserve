namespace ReserveFlow.Modules.Notifications.Presentation;

internal sealed record QueueNotificationRequest(
    Guid TenantId,
    string Channel,
    string Recipient,
    string Subject,
    string Body,
    DateTime? DeliverAtUtc = null,
    string? CorrelationKey = null);
