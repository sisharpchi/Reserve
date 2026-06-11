using ReserveFlow.Modules.Bookings.Domain.Bookings;

namespace ReserveFlow.UnitTests.Bookings;

public sealed class BookingTests
{
    [Fact]
    public void CreateRaisesCreatedEventAndGeneratesPublicLookup()
    {
        Guid tenantId = Guid.NewGuid();
        DateTimeOffset startsAtUtc = new(2026, 6, 1, 9, 0, 0, TimeSpan.Zero);
        DateTimeOffset endsAtUtc = startsAtUtc.AddMinutes(30);

        Booking booking = CreateBooking(
            tenantId: tenantId,
            startsAtUtc: startsAtUtc,
            endsAtUtc: endsAtUtc,
            idempotencyKey: "  checkout-123  ");

        Assert.Equal(tenantId, booking.TenantId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Equal("checkout-123", booking.IdempotencyKey);
        Assert.StartsWith("RF-", booking.PublicReference, StringComparison.Ordinal);
        Assert.Equal(13, booking.PublicReference.Length);
        Assert.False(string.IsNullOrWhiteSpace(booking.AccessToken));
        Assert.NotEqual(Guid.Empty, booking.ConcurrencyToken);

        BookingCreatedDomainEvent domainEvent = Assert.IsType<BookingCreatedDomainEvent>(
            Assert.Single(booking.DomainEvents));

        Assert.Equal(booking.Id, domainEvent.BookingId);
        Assert.Equal(tenantId, domainEvent.TenantId);
        Assert.Equal(startsAtUtc, domainEvent.StartsAtUtc);
        Assert.Equal(endsAtUtc, domainEvent.EndsAtUtc);
    }

    [Fact]
    public void CancelledBookingCannotBeRescheduled()
    {
        Booking booking = CreateBooking();
        booking.Cancel(Now);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            booking.Reschedule(Now.AddDays(2), Now.AddDays(2).AddMinutes(30)));

        Assert.Equal("Cancelled booking cannot be rescheduled.", exception.Message);
    }

    [Fact]
    public void CompletedBookingCannotBeCancelled()
    {
        Booking booking = CreateBooking();
        booking.Confirm(Now);
        booking.Complete(Now.AddMinutes(30));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            booking.Cancel(Now.AddMinutes(31)));

        Assert.Equal("Completed booking cannot be cancelled.", exception.Message);
    }

    [Fact]
    public void LifecycleMutationsRefreshConcurrencyToken()
    {
        Booking booking = CreateBooking();
        Guid createdToken = booking.ConcurrencyToken;

        booking.Confirm(Now);

        Guid confirmedToken = booking.ConcurrencyToken;

        booking.Reschedule(Now.AddDays(2), Now.AddDays(2).AddMinutes(30));

        Guid rescheduledToken = booking.ConcurrencyToken;

        booking.MarkAsNoShow(Now.AddDays(2).AddMinutes(31));

        Assert.NotEqual(createdToken, confirmedToken);
        Assert.NotEqual(confirmedToken, rescheduledToken);
        Assert.NotEqual(rescheduledToken, booking.ConcurrencyToken);
    }

    [Fact]
    public void CreateRejectsInvalidTimeRange()
    {
        DateTimeOffset startsAtUtc = Now;

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            CreateBooking(startsAtUtc: startsAtUtc, endsAtUtc: startsAtUtc));

        Assert.Equal("Booking end time must be after start time.", exception.Message);
    }

    private static readonly DateTimeOffset Now = new(2026, 6, 1, 9, 0, 0, TimeSpan.Zero);

    private static Booking CreateBooking(
        Guid? tenantId = null,
        DateTimeOffset? startsAtUtc = null,
        DateTimeOffset? endsAtUtc = null,
        string? idempotencyKey = null)
    {
        DateTimeOffset start = startsAtUtc ?? Now.AddDays(1);

        return Booking.Create(
            tenantId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            staffMemberId: Guid.NewGuid(),
            resourceId: Guid.NewGuid(),
            start,
            endsAtUtc ?? start.AddMinutes(30),
            idempotencyKey);
    }
}
