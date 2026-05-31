using ReserveFlow.Common.Domain;
using ReserveFlow.Modules.Scheduling.Domain.Availability;

namespace ReserveFlow.Modules.Scheduling.Domain.WorkingHours;

public sealed class WorkingHour : Entity
{
    private WorkingHour(
        Guid id,
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DayOfWeek dayOfWeek,
        TimeOnly startsAt,
        TimeOnly endsAt)
        : base(id)
    {
        TenantId = tenantId;
        StaffMemberId = staffMemberId;
        ResourceId = resourceId;
        DayOfWeek = dayOfWeek;
        StartsAt = startsAt;
        EndsAt = endsAt;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private WorkingHour()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid? StaffMemberId { get; private set; }

    public Guid? ResourceId { get; private set; }

    public DayOfWeek DayOfWeek { get; private set; }

    public TimeOnly StartsAt { get; private set; }

    public TimeOnly EndsAt { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static WorkingHour Create(
        Guid tenantId,
        Guid? staffMemberId,
        Guid? resourceId,
        DayOfWeek dayOfWeek,
        TimeOnly startsAt,
        TimeOnly endsAt)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id is required.");
        }

        if (staffMemberId is null && resourceId is null)
        {
            throw new InvalidOperationException("Working hour must target staff or resource.");
        }

        if (endsAt <= startsAt)
        {
            throw new InvalidOperationException("Working hour end must be after start.");
        }

        var workingHour = new WorkingHour(
            Guid.NewGuid(),
            tenantId,
            staffMemberId,
            resourceId,
            dayOfWeek,
            startsAt,
            endsAt);

        workingHour.RaiseDomainEvent(new WorkingHourCreatedDomainEvent(
            workingHour.Id,
            workingHour.TenantId,
            workingHour.DayOfWeek));

        return workingHour;
    }

    public AvailabilityWindow ToAvailabilityWindow(DateOnly date)
    {
        if (date.DayOfWeek != DayOfWeek)
        {
            throw new InvalidOperationException("Working hour day does not match the requested date.");
        }

        return new AvailabilityWindow(
            new DateTimeOffset(date.ToDateTime(StartsAt), TimeSpan.Zero),
            new DateTimeOffset(date.ToDateTime(EndsAt), TimeSpan.Zero));
    }
}
