using System.Text.Json;
using ReserveFlow.Common.Domain;

namespace ReserveFlow.Common.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
        Type = string.Empty;
        Payload = string.Empty;
    }

    private OutboxMessage(
        Guid id,
        Guid? tenantId,
        DateTime occurredOnUtc,
        string type,
        string payload)
    {
        Id = id;
        TenantId = tenantId;
        OccurredOnUtc = occurredOnUtc;
        Type = type;
        Payload = payload;
        RetryCount = 0;
    }

    public Guid Id { get; private set; }

    public Guid? TenantId { get; private set; }

    public DateTime OccurredOnUtc { get; private set; }

    public string Type { get; private set; }

    public string Payload { get; private set; }

    public DateTime? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public int RetryCount { get; private set; }

    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        Type eventType = domainEvent.GetType();

        return new OutboxMessage(
            Guid.NewGuid(),
            ResolveTenantId(domainEvent),
            domainEvent.OccurredOnUtc,
            eventType.FullName ?? eventType.Name,
            JsonSerializer.Serialize(domainEvent, eventType));
    }

    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = null;
    }

    public void RecordFailure(string error)
    {
        RetryCount++;
        Error = error;
    }

    private static Guid? ResolveTenantId(IDomainEvent domainEvent)
    {
        object? value = domainEvent.GetType().GetProperty("TenantId")?.GetValue(domainEvent);

        return value is Guid tenantId ? tenantId : null;
    }
}
