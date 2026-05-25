using System.Text.Json;
using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Integrations.Domain.WebhookInbox;

public sealed class WebhookInboxMessage : Entity
{
    private WebhookInboxMessage(
        Guid id,
        Guid? tenantId,
        string source,
        string externalMessageId,
        string eventType,
        string payloadJson)
        : base(id)
    {
        TenantId = tenantId;
        Source = source;
        ExternalMessageId = externalMessageId;
        EventType = eventType;
        PayloadJson = payloadJson;
        Status = WebhookInboxStatus.Received;
        ReceivedAtUtc = DateTime.UtcNow;
    }

    private WebhookInboxMessage()
    {
    }

    public Guid? TenantId { get; private set; }

    public string Source { get; private set; } = string.Empty;

    public string ExternalMessageId { get; private set; } = string.Empty;

    public string EventType { get; private set; } = string.Empty;

    public string PayloadJson { get; private set; } = "{}";

    public WebhookInboxStatus Status { get; private set; }

    public DateTime ReceivedAtUtc { get; private set; }

    public DateTime? ProcessedAtUtc { get; private set; }

    public string? Error { get; private set; }

    public static WebhookInboxMessage Accept(
        Guid? tenantId,
        string source,
        string externalMessageId,
        string eventType,
        string payloadJson)
    {
        string normalizedSource = NormalizeSource(source);
        string normalizedExternalMessageId = NormalizeRequired(externalMessageId, "External message id");
        string normalizedEventType = NormalizeRequired(eventType, "Webhook event type");
        string normalizedPayload = NormalizeJson(payloadJson);

        var message = new WebhookInboxMessage(
            Guid.NewGuid(),
            tenantId,
            normalizedSource,
            normalizedExternalMessageId,
            normalizedEventType,
            normalizedPayload);

        message.RaiseDomainEvent(new WebhookInboxMessageReceivedDomainEvent(
            message.Id,
            tenantId,
            normalizedSource,
            normalizedExternalMessageId,
            normalizedEventType));

        return message;
    }

    public void MarkProcessed(DateTime processedAtUtc)
    {
        Status = WebhookInboxStatus.Processed;
        ProcessedAtUtc = processedAtUtc;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Status = WebhookInboxStatus.Failed;
        Error = NormalizeRequired(error, "Webhook inbox error");
    }

    public static string NormalizeSource(string source)
    {
        return NormalizeRequired(source, "Webhook source").ToLowerInvariant();
    }

    public static string NormalizeExternalMessageId(string externalMessageId)
    {
        return NormalizeRequired(externalMessageId, "External message id");
    }

    private static string NormalizeJson(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "{}";
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(value);

            return document.RootElement.GetRawText();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Webhook payload must be valid JSON.", exception);
        }
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
