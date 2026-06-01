namespace ReserveFlow.Modules.Bookings.Application.Bookings;

public interface ITenantBookingGate
{
    Task<bool> CanAcceptPublicBookingAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
