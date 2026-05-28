using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Scheduling.Domain.UnavailablePeriods;

public sealed class UnavailablePeriod : Entity
{
    private UnavailablePeriod(
        Guid id,
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        string? reason)
        : base(id)
    {
        TenantId = tenantId;
        StaffMemberId = staffMemberId;
        ResourceId = resourceId;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Reason = reason;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private UnavailablePeriod()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid? StaffMemberId { get; private set; }

    public Guid? ResourceId { get; private set; }

    public DateTimeOffset StartsAtUtc { get; private set; }

    public DateTimeOffset EndsAtUtc { get; private set; }

    public string? Reason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static UnavailablePeriod Create(
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        string? reason)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id is required.");
        }

        if (staffMemberId is null && resourceId is null)
        {
            throw new InvalidOperationException("Unavailable period must target staff or resource.");
        }

        if (endsAtUtc <= startsAtUtc)
        {
            throw new InvalidOperationException("Unavailable period end must be after start.");
        }

        var unavailablePeriod = new UnavailablePeriod(
            Guid.NewGuid(),
            tenantId,
            staffMemberId,
            resourceId,
            startsAtUtc,
            endsAtUtc,
            string.IsNullOrWhiteSpace(reason) ? null : reason.Trim());

        unavailablePeriod.RaiseDomainEvent(new UnavailablePeriodCreatedDomainEvent(
            unavailablePeriod.Id,
            unavailablePeriod.TenantId,
            unavailablePeriod.StartsAtUtc,
            unavailablePeriod.EndsAtUtc));

        return unavailablePeriod;
    }
}
