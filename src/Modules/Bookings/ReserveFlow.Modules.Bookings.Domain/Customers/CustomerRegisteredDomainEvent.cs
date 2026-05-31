using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Bookings.Domain.Customers;

public sealed record CustomerRegisteredDomainEvent(
    Guid CustomerId,
    Guid TenantId,
    string Email) : DomainEvent;
