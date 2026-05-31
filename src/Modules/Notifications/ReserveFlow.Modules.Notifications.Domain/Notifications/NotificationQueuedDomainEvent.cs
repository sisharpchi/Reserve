using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Notifications.Domain.Notifications;

public sealed record NotificationQueuedDomainEvent(
    Guid NotificationId,
    Guid TenantId,
    string Channel,
    string Recipient) : DomainEvent;
