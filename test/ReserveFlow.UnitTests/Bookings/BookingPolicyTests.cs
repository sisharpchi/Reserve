using ReserveFlow.Modules.Bookings.Domain.BookingPolicies;

namespace ReserveFlow.UnitTests.Bookings;

public sealed class BookingPolicyTests
{
    [Fact]
    public void MinimumAdvancePolicyBlocksTooSoonBooking()
    {
        BookingPolicy policy = BookingPolicy.Configure(
            Guid.NewGuid(),
            minimumAdvanceMinutes: 120,
            cancellationDeadlineHours: 12);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            policy.EnsureBookingCanStartAt(Now.AddMinutes(119), Now));

        Assert.Equal("Booking violates the tenant minimum advance policy.", exception.Message);
    }

    [Fact]
    public void CancellationDeadlineBlocksLateCancellation()
    {
        BookingPolicy policy = BookingPolicy.Configure(
            Guid.NewGuid(),
            minimumAdvanceMinutes: 30,
            cancellationDeadlineHours: 12);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            policy.EnsureCancellationAllowed(Now.AddHours(12), Now.AddMinutes(1)));

        Assert.Equal("Booking cancellation deadline has passed.", exception.Message);
    }

    [Fact]
    public void ConfigureRejectsNegativeRules()
    {
        Assert.Throws<InvalidOperationException>(() =>
            BookingPolicy.Configure(Guid.NewGuid(), minimumAdvanceMinutes: -1, cancellationDeadlineHours: 12));

        Assert.Throws<InvalidOperationException>(() =>
            BookingPolicy.Configure(Guid.NewGuid(), minimumAdvanceMinutes: 30, cancellationDeadlineHours: -1));
    }

    [Fact]
    public void UpdateRaisesConfiguredEventWithNewRules()
    {
        Guid tenantId = Guid.NewGuid();
        BookingPolicy policy = BookingPolicy.Configure(
            tenantId,
            minimumAdvanceMinutes: 30,
            cancellationDeadlineHours: 12);

        policy.ClearDomainEvents();

        policy.Update(minimumAdvanceMinutes: 60, cancellationDeadlineHours: 24);

        Assert.Equal(60, policy.MinimumAdvanceMinutes);
        Assert.Equal(24, policy.CancellationDeadlineHours);

        BookingPolicyConfiguredDomainEvent domainEvent = Assert.IsType<BookingPolicyConfiguredDomainEvent>(
            Assert.Single(policy.DomainEvents));

        Assert.Equal(policy.Id, domainEvent.BookingPolicyId);
        Assert.Equal(tenantId, domainEvent.TenantId);
        Assert.Equal(60, domainEvent.MinimumAdvanceMinutes);
        Assert.Equal(24, domainEvent.CancellationDeadlineHours);
    }

    private static readonly DateTimeOffset Now = new(2026, 6, 1, 9, 0, 0, TimeSpan.Zero);
}
