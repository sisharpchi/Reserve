namespace ReserveFlow.Modules.Bookings.Application.Bookings;

public interface IBookingsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
