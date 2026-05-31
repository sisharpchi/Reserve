using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Integrations.Domain.WebhookInbox;

public sealed record WebhookInboxMessageReceivedDomainEvent(
    Guid InboxMessageId,
    Guid? TenantId,
    string Source,
    string ExternalMessageId,
    string EventType) : DomainEvent;
