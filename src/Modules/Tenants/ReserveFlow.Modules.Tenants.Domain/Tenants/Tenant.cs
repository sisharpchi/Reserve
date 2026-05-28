using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Tenants.Domain.Tenants;

public sealed class Tenant : Entity
{
    private Tenant(
        Guid id,
        string name,
        string slug,
        string timeZoneId,
        Guid? categoryId)
        : base(id)
    {
        Name = name;
        Slug = slug;
        TimeZoneId = timeZoneId;
        CategoryId = categoryId;
        Status = TenantStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private Tenant()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string TimeZoneId { get; private set; } = string.Empty;

    public Guid? CategoryId { get; private set; }

    public TenantStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static Tenant Create(
        string name,
        string slug,
        string timeZoneId,
        Guid? categoryId = null)
    {
        string normalizedName = NormalizeRequired(name, "Tenant name");
        string normalizedSlug = NormalizeSlug(slug);
        string normalizedTimeZoneId = NormalizeRequired(timeZoneId, "Tenant time zone");

        if (categoryId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant category id cannot be empty.");
        }

        var tenant = new Tenant(
            Guid.NewGuid(),
            normalizedName,
            normalizedSlug,
            normalizedTimeZoneId,
            categoryId);

        tenant.RaiseDomainEvent(new TenantProvisionedDomainEvent(
            tenant.Id,
            tenant.Slug,
            tenant.TimeZoneId,
            tenant.CategoryId));

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

    public void Activate(DateTimeOffset activatedAtUtc)
    {
        if (Status is TenantStatus.Active)
        {
            throw new InvalidOperationException("Tenant is already active.");
        }

        if (Status is TenantStatus.Deleted)
        {
            throw new InvalidOperationException("Deleted tenant cannot be activated.");
        }

        Status = TenantStatus.Active;
        RaiseDomainEvent(new TenantActivatedDomainEvent(Id, activatedAtUtc));
    }

    public void Suspend(DateTimeOffset suspendedAtUtc)
    {
        if (Status is TenantStatus.Suspended)
        {
            throw new InvalidOperationException("Tenant is already suspended.");
        }

        if (Status is TenantStatus.Deleted)
        {
            throw new InvalidOperationException("Deleted tenant cannot be suspended.");
        }

        Status = TenantStatus.Suspended;
        RaiseDomainEvent(new TenantSuspendedDomainEvent(Id, suspendedAtUtc));
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
