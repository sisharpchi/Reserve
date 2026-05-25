using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Bookings.Application.Customers;
using ReserveFlow.Modules.Bookings.Domain.Customers;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;

namespace ReserveFlow.Modules.Bookings.Infrastructure.Customers;

internal sealed class CustomerRepository(BookingsDbContext dbContext) : ICustomerRepository
{
    public void Insert(Customer customer)
    {
        dbContext.Customers.Add(customer);
    }

    public async Task<Customer?> FindByEmailAsync(
        Guid tenantId,
        string email,
        CancellationToken cancellationToken = default)
    {
        string normalizedEmail = Customer.NormalizeEmail(email);

        return await dbContext.Customers.FirstOrDefaultAsync(
            customer => customer.TenantId == tenantId && customer.Email == normalizedEmail,
            cancellationToken);
    }
}
