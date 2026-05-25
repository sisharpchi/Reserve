using System.Text.Json;
using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Audit.Domain.AuditLogs;

public sealed class AuditLog : Entity
{
    private AuditLog(
        Guid id,
        Guid? tenantId,
        Guid? userId,
        string action,
        string entityName,
        Guid? entityId,
        string detailsJson)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        DetailsJson = detailsJson;
        OccurredOnUtc = DateTime.UtcNow;
    }

    private AuditLog()
    {
    }

    public Guid? TenantId { get; private set; }

    public Guid? UserId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string EntityName { get; private set; } = string.Empty;

    public Guid? EntityId { get; private set; }

    public string DetailsJson { get; private set; } = "{}";

    public DateTime OccurredOnUtc { get; private set; }

    public static AuditLog Record(
        Guid? tenantId,
        Guid? userId,
        string action,
        string entityName,
        Guid? entityId,
        string detailsJson)
    {
        string normalizedAction = NormalizeRequired(action, "Audit action");
        string normalizedEntityName = NormalizeRequired(entityName, "Audit entity name");
        string normalizedDetails = NormalizeJson(detailsJson);

        var log = new AuditLog(
            Guid.NewGuid(),
            tenantId,
            userId,
            normalizedAction,
            normalizedEntityName,
            entityId,
            normalizedDetails);

        log.RaiseDomainEvent(new AuditLogRecordedDomainEvent(
            log.Id,
            tenantId,
            normalizedAction,
            normalizedEntityName));

        return log;
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
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
            throw new InvalidOperationException("Audit details must be valid JSON.", exception);
        }
    }
}
