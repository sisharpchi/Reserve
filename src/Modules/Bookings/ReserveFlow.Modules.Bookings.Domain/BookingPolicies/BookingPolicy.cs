using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Bookings.Domain.BookingPolicies;

public sealed class BookingPolicy : Entity
{
    private BookingPolicy(
        Guid id,
        Guid tenantId,
        int minimumAdvanceMinutes,
        int cancellationDeadlineHours)
        : base(id)
    {
        TenantId = tenantId;
        MinimumAdvanceMinutes = minimumAdvanceMinutes;
        CancellationDeadlineHours = cancellationDeadlineHours;
    }

    private BookingPolicy()
    {
    }

    public Guid TenantId { get; private set; }

    public int MinimumAdvanceMinutes { get; private set; }

    public int CancellationDeadlineHours { get; private set; }

    public static BookingPolicy Configure(
        Guid tenantId,
        int minimumAdvanceMinutes,
        int cancellationDeadlineHours)
    {
        ValidateRules(minimumAdvanceMinutes, cancellationDeadlineHours);

        var policy = new BookingPolicy(
            Guid.NewGuid(),
            tenantId,
            minimumAdvanceMinutes,
            cancellationDeadlineHours);

        policy.RaiseDomainEvent(new BookingPolicyConfiguredDomainEvent(
            policy.Id,
            tenantId,
            minimumAdvanceMinutes,
            cancellationDeadlineHours));

        return policy;
    }

    public void Update(
        int minimumAdvanceMinutes,
        int cancellationDeadlineHours)
    {
        ValidateRules(minimumAdvanceMinutes, cancellationDeadlineHours);

        MinimumAdvanceMinutes = minimumAdvanceMinutes;
        CancellationDeadlineHours = cancellationDeadlineHours;

        RaiseDomainEvent(new BookingPolicyConfiguredDomainEvent(
            Id,
            TenantId,
            minimumAdvanceMinutes,
            cancellationDeadlineHours));
    }

    public void EnsureBookingCanStartAt(
        DateTimeOffset startsAtUtc,
        DateTimeOffset nowUtc)
    {
        DateTimeOffset earliestAllowedStart = nowUtc.AddMinutes(MinimumAdvanceMinutes);

        if (startsAtUtc < earliestAllowedStart)
        {
            throw new InvalidOperationException("Booking violates the tenant minimum advance policy.");
        }
    }

    public void EnsureCancellationAllowed(
        DateTimeOffset startsAtUtc,
        DateTimeOffset nowUtc)
    {
        DateTimeOffset cancellationDeadline = startsAtUtc.AddHours(-CancellationDeadlineHours);

        if (nowUtc > cancellationDeadline)
        {
            throw new InvalidOperationException("Booking cancellation deadline has passed.");
        }
    }

    private static void ValidateRules(
        int minimumAdvanceMinutes,
        int cancellationDeadlineHours)
    {
        if (minimumAdvanceMinutes < 0)
        {
            throw new InvalidOperationException("Minimum advance minutes cannot be negative.");
        }

        if (cancellationDeadlineHours < 0)
        {
            throw new InvalidOperationException("Cancellation deadline hours cannot be negative.");
        }
    }
}
