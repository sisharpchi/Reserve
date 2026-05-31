using ReserveFlow.Modules.Integrations.Domain.WebhookInbox;

namespace ReserveFlow.Modules.Integrations.Application.WebhookInbox;

public sealed record WebhookInboxMessageResponse(
    Guid Id,
    Guid? TenantId,
    string Source,
    string ExternalMessageId,
    string EventType,
    string PayloadJson,
    string Status,
    DateTime ReceivedAtUtc,
    DateTime? ProcessedAtUtc,
    string? Error,
    bool IsDuplicate)
{
    public static WebhookInboxMessageResponse FromMessage(
        WebhookInboxMessage message,
        bool isDuplicate)
    {
        return new WebhookInboxMessageResponse(
            message.Id,
            message.TenantId,
            message.Source,
            message.ExternalMessageId,
            message.EventType,
            message.PayloadJson,
            message.Status.ToString(),
            message.ReceivedAtUtc,
            message.ProcessedAtUtc,
            message.Error,
            isDuplicate);
    }
}
