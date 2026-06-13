using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Notifications.Domain.Notifications;

public sealed class NotificationMessage : Entity
{
    private NotificationMessage(
        Guid id,
        Guid tenantId,
        NotificationChannel channel,
        string recipient,
        string subject,
        string body,
        DateTime deliverAtUtc,
        string? correlationKey)
        : base(id)
    {
        TenantId = tenantId;
        Channel = channel;
        Recipient = recipient;
        Subject = subject;
        Body = body;
        Status = NotificationStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        DeliverAtUtc = deliverAtUtc;
        CorrelationKey = correlationKey;
    }

    private NotificationMessage()
    {
    }

    public Guid TenantId { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public string Recipient { get; private set; } = string.Empty;

    public string Subject { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public NotificationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime DeliverAtUtc { get; private set; }

    public string? CorrelationKey { get; private set; }

    public DateTime? SentAtUtc { get; private set; }

    public string? Error { get; private set; }

    public static NotificationMessage Queue(
        Guid tenantId,
        NotificationChannel channel,
        string recipient,
        string subject,
        string body,
        DateTime? deliverAtUtc = null,
        string? correlationKey = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id is required.");
        }

        string normalizedRecipient = NormalizeRecipient(channel, recipient);
        string normalizedSubject = NormalizeOptional(subject);
        string normalizedBody = NormalizeRequired(body, "Notification body");
        DateTime normalizedDeliverAtUtc = NormalizeDeliverAt(deliverAtUtc);
        string? normalizedCorrelationKey = NormalizeOptionalNullable(correlationKey);

        var message = new NotificationMessage(
            Guid.NewGuid(),
            tenantId,
            channel,
            normalizedRecipient,
            normalizedSubject,
            normalizedBody,
            normalizedDeliverAtUtc,
            normalizedCorrelationKey);

        message.RaiseDomainEvent(new NotificationQueuedDomainEvent(
            message.Id,
            tenantId,
            channel.ToString(),
            normalizedRecipient));

        return message;
    }

    public void MarkSent(DateTime sentAtUtc)
    {
        Status = NotificationStatus.Sent;
        SentAtUtc = sentAtUtc;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Status = NotificationStatus.Failed;
        Error = NormalizeRequired(error, "Notification error");
    }

    public void Cancel(string reason)
    {
        if (Status is not NotificationStatus.Pending)
        {
            return;
        }

        Status = NotificationStatus.Cancelled;
        Error = NormalizeRequired(reason, "Notification cancellation reason");
    }

    private static string NormalizeRecipient(NotificationChannel channel, string recipient)
    {
        string normalized = NormalizeRequired(recipient, "Notification recipient");

        return channel is NotificationChannel.Email
            ? normalized.ToLowerInvariant()
            : normalized;
    }

    private static string NormalizeOptional(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string? NormalizeOptionalNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static DateTime NormalizeDeliverAt(DateTime? deliverAtUtc)
    {
        return deliverAtUtc?.ToUniversalTime() ?? DateTime.UtcNow;
    }
}
