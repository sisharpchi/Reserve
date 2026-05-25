using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Bookings.Domain.Customers;

public sealed class Customer : Entity
{
    private Customer(
        Guid id,
        Guid tenantId,
        string fullName,
        string email,
        string? phoneNumber,
        DateTimeOffset registeredAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;
        RegisteredAtUtc = registeredAtUtc;
    }

    private Customer()
    {
    }

    public Guid TenantId { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? PhoneNumber { get; private set; }

    public DateTimeOffset RegisteredAtUtc { get; private set; }

    public static Customer Register(
        Guid tenantId,
        string fullName,
        string email,
        string? phoneNumber)
    {
        string normalizedFullName = NormalizeFullName(fullName);
        string normalizedEmail = NormalizeEmail(email);
        string? normalizedPhoneNumber = NormalizePhoneNumber(phoneNumber);

        var customer = new Customer(
            Guid.NewGuid(),
            tenantId,
            normalizedFullName,
            normalizedEmail,
            normalizedPhoneNumber,
            DateTimeOffset.UtcNow);

        customer.RaiseDomainEvent(new CustomerRegisteredDomainEvent(customer.Id, tenantId, normalizedEmail));

        return customer;
    }

    public static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Customer email is required.");
        }

        return email.Trim().ToLowerInvariant();
    }

    private static string NormalizeFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Customer full name is required.");
        }

        return fullName.Trim();
    }

    private static string? NormalizePhoneNumber(string? phoneNumber)
    {
        return string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
    }
}
