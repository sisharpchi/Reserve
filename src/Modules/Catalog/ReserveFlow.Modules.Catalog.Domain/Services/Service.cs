using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Catalog.Domain.Services;

public sealed class Service : Entity
{
    private Service(
        Guid id,
        Guid tenantId,
        string name,
        int durationMinutes,
        decimal? price,
        string? currency)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        DurationMinutes = durationMinutes;
        Price = price;
        Currency = currency;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private Service()
    {
    }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int DurationMinutes { get; private set; }

    public decimal? Price { get; private set; }

    public string? Currency { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static Service Create(
        Guid tenantId,
        string name,
        int durationMinutes,
        decimal? price,
        string? currency)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id is required.");
        }

        string normalizedName = NormalizeRequired(name, "Service name");

        if (durationMinutes <= 0)
        {
            throw new InvalidOperationException("Service duration must be greater than zero.");
        }

        if (price < 0)
        {
            throw new InvalidOperationException("Service price cannot be negative.");
        }

        var service = new Service(
            Guid.NewGuid(),
            tenantId,
            normalizedName,
            durationMinutes,
            price,
            NormalizeCurrency(currency));

        service.RaiseDomainEvent(new ServiceCreatedDomainEvent(
            service.Id,
            service.TenantId,
            service.Name,
            service.DurationMinutes));

        return service;
    }

    public void Update(
        string name,
        int durationMinutes,
        decimal? price,
        string? currency)
    {
        string normalizedName = NormalizeRequired(name, "Service name");

        if (durationMinutes <= 0)
        {
            throw new InvalidOperationException("Service duration must be greater than zero.");
        }

        if (price < 0)
        {
            throw new InvalidOperationException("Service price cannot be negative.");
        }

        Name = normalizedName;
        DurationMinutes = durationMinutes;
        Price = price;
        Currency = NormalizeCurrency(currency);

        RaiseDomainEvent(new ServiceUpdatedDomainEvent(Id, TenantId, Name, DurationMinutes));
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Service is already inactive.");
        }

        IsActive = false;
        RaiseDomainEvent(new ServiceDeactivatedDomainEvent(Id, TenantId));
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? null
            : currency.Trim().ToUpperInvariant();
    }
}
