using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Integrations.Application.WebhookInbox.AcceptWebhook;

public sealed record AcceptWebhookCommand(
    Guid? TenantId,
    string Source,
    string ExternalMessageId,
    string EventType,
    string PayloadJson) : ICommand<WebhookInboxMessageResponse>;
