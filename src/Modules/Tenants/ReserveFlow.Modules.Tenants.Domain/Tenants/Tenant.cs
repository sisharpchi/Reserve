using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Tenants.Domain.Tenants;

public sealed class Tenant : Entity
{
    private Tenant(
        Guid id,
        string name,
        string slug,
        string timeZoneId)
        : base(id)
    {
        Name = name;
        Slug = slug;
        TimeZoneId = timeZoneId;
        Status = TenantStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private Tenant()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string TimeZoneId { get; private set; } = string.Empty;

    public TenantStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static Tenant Create(string name, string slug, string timeZoneId)
    {
        string normalizedName = NormalizeRequired(name, "Tenant name");
        string normalizedSlug = NormalizeSlug(slug);
        string normalizedTimeZoneId = NormalizeRequired(timeZoneId, "Tenant time zone");

        var tenant = new Tenant(Guid.NewGuid(), normalizedName, normalizedSlug, normalizedTimeZoneId);

        tenant.RaiseDomainEvent(new TenantProvisionedDomainEvent(
            tenant.Id,
            tenant.Slug,
            tenant.TimeZoneId));

        return tenant;
    }

    public static string NormalizeSlug(string slug)
    {
        string normalizedSlug = NormalizeRequired(slug, "Tenant slug").ToLowerInvariant();

        if (normalizedSlug.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-'))
        {
            throw new InvalidOperationException("Tenant slug can contain only letters, digits, and hyphens.");
        }

        return normalizedSlug;
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }
}
