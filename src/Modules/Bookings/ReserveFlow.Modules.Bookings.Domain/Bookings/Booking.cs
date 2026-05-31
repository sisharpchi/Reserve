using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Bookings.Domain.Bookings;

public sealed class Booking : Entity
{
    private Booking(
        Guid id,
        Guid tenantId,
        Guid customerId,
        Guid serviceId,
        Guid? staffMemberId,
        Guid? resourceId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        string? idempotencyKey)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        ServiceId = serviceId;
        StaffMemberId = staffMemberId;
        ResourceId = resourceId;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        IdempotencyKey = idempotencyKey;
        PublicReference = GeneratePublicReference();
        AccessToken = GenerateAccessToken();
        ConcurrencyToken = Guid.NewGuid();
        Status = BookingStatus.Pending;
    }

    private Booking()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid CustomerId { get; private set; }

    public Guid ServiceId { get; private set; }

    public Guid? StaffMemberId { get; private set; }

    public Guid? ResourceId { get; private set; }

    public DateTimeOffset StartsAtUtc { get; private set; }

    public DateTimeOffset EndsAtUtc { get; private set; }

    public BookingStatus Status { get; private set; }

    public string? IdempotencyKey { get; private set; }

    public string PublicReference { get; private set; } = string.Empty;

    public string AccessToken { get; private set; } = string.Empty;

    public Guid ConcurrencyToken { get; private set; } = Guid.NewGuid();

    public static Booking Create(
        Guid tenantId,
        Guid customerId,
        Guid serviceId,
        Guid? staffMemberId,
        Guid? resourceId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        string? idempotencyKey = null)
    {
        if (endsAtUtc <= startsAtUtc)
        {
            throw new InvalidOperationException("Booking end time must be after start time.");
        }

        var booking = new Booking(
            Guid.NewGuid(),
            tenantId,
            customerId,
            serviceId,
            staffMemberId,
            resourceId,
            startsAtUtc,
            endsAtUtc,
            NormalizeIdempotencyKey(idempotencyKey));

        booking.RaiseDomainEvent(new BookingCreatedDomainEvent(booking.Id, tenantId, startsAtUtc, endsAtUtc));

        return booking;
    }

    public static string? NormalizeIdempotencyKey(string? idempotencyKey)
    {
        return string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();
    }

    public static string NormalizePublicReference(string publicReference)
    {
        return publicReference.Trim().ToUpperInvariant();
    }

    public void Cancel(DateTimeOffset cancelledAtUtc)
    {
        if (Status is BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Booking is already cancelled.");
        }

        if (Status is BookingStatus.Completed)
        {
            throw new InvalidOperationException("Completed booking cannot be cancelled.");
        }

        Status = BookingStatus.Cancelled;
        RefreshConcurrencyToken();
        RaiseDomainEvent(new BookingCancelledDomainEvent(Id, TenantId, cancelledAtUtc));
    }

    public void Reschedule(DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc)
    {
        if (endsAtUtc <= startsAtUtc)
        {
            throw new InvalidOperationException("Booking end time must be after start time.");
        }

        if (Status is BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled booking cannot be rescheduled.");
        }

        if (Status is BookingStatus.Completed)
        {
            throw new InvalidOperationException("Completed booking cannot be rescheduled.");
        }

        if (Status is BookingStatus.NoShow)
        {
            throw new InvalidOperationException("No-show booking cannot be rescheduled.");
        }

        if (Status is BookingStatus.Expired)
        {
            throw new InvalidOperationException("Expired booking cannot be rescheduled.");
        }

        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Status = BookingStatus.Rescheduled;
        RefreshConcurrencyToken();

        RaiseDomainEvent(new BookingRescheduledDomainEvent(Id, TenantId, startsAtUtc, endsAtUtc));
    }

    public void Confirm(DateTimeOffset confirmedAtUtc)
    {
        if (Status is BookingStatus.Confirmed)
        {
            throw new InvalidOperationException("Booking is already confirmed.");
        }

        if (Status is BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled booking cannot be confirmed.");
        }

        if (Status is BookingStatus.Completed)
        {
            throw new InvalidOperationException("Completed booking cannot be confirmed.");
        }

        if (Status is BookingStatus.NoShow)
        {
            throw new InvalidOperationException("No-show booking cannot be confirmed.");
        }

        if (Status is BookingStatus.Expired)
        {
            throw new InvalidOperationException("Expired booking cannot be confirmed.");
        }

        Status = BookingStatus.Confirmed;
        RefreshConcurrencyToken();
        RaiseDomainEvent(new BookingConfirmedDomainEvent(Id, TenantId, confirmedAtUtc));
    }

    public void Expire(DateTimeOffset expiredAtUtc)
    {
        if (Status is not BookingStatus.Pending)
        {
            throw new InvalidOperationException("Only pending bookings can be expired.");
        }

        Status = BookingStatus.Expired;
        RefreshConcurrencyToken();
        RaiseDomainEvent(new BookingExpiredDomainEvent(Id, TenantId, expiredAtUtc));
    }

    public void Complete(DateTimeOffset completedAtUtc)
    {
        if (Status is BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled booking cannot be completed.");
        }

        if (Status is BookingStatus.Completed)
        {
            throw new InvalidOperationException("Booking is already completed.");
        }

        if (Status is BookingStatus.NoShow)
        {
            throw new InvalidOperationException("No-show booking cannot be completed.");
        }

        if (Status is BookingStatus.Expired)
        {
            throw new InvalidOperationException("Expired booking cannot be completed.");
        }

        Status = BookingStatus.Completed;
        RefreshConcurrencyToken();
        RaiseDomainEvent(new BookingCompletedDomainEvent(Id, TenantId, completedAtUtc));
    }

    public void MarkAsNoShow(DateTimeOffset markedAtUtc)
    {
        if (Status is BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled booking cannot be marked as no-show.");
        }

        if (Status is BookingStatus.Completed)
        {
            throw new InvalidOperationException("Completed booking cannot be marked as no-show.");
        }

        if (Status is BookingStatus.NoShow)
        {
            throw new InvalidOperationException("Booking is already marked as no-show.");
        }

        if (Status is BookingStatus.Expired)
        {
            throw new InvalidOperationException("Expired booking cannot be marked as no-show.");
        }

        Status = BookingStatus.NoShow;
        RefreshConcurrencyToken();
        RaiseDomainEvent(new BookingMarkedAsNoShowDomainEvent(Id, TenantId, markedAtUtc));
    }

    private void RefreshConcurrencyToken()
    {
        ConcurrencyToken = Guid.NewGuid();
    }

    private static string GeneratePublicReference()
    {
        return $"RF-{Guid.NewGuid():N}"[..13].ToUpperInvariant();
    }

    private static string GenerateAccessToken()
    {
        return Guid.NewGuid().ToString("N");
    }
}
