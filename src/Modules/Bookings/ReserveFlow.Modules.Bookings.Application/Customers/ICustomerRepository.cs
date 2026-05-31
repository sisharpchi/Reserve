using ReserveFlow.Modules.Bookings.Domain.Customers;

namespace ReserveFlow.Modules.Bookings.Application.Customers;

public interface ICustomerRepository
{
    void Insert(Customer customer);

    Task<Customer?> FindByEmailAsync(
        Guid tenantId,
        string email,
        CancellationToken cancellationToken = default);
}
