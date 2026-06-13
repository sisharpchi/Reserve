using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Tenants.Domain.TenantCategories;

public sealed class TenantCategory : Entity
{
    private TenantCategory(
        Guid id,
        string name,
        string slug,
        int sortOrder)
        : base(id)
    {
        Name = name;
        Slug = slug;
        SortOrder = sortOrder;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private TenantCategory()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static TenantCategory Create(string name, string slug, int sortOrder = 0)
    {
        string normalizedName = NormalizeRequired(name, "Tenant category name");
        string normalizedSlug = NormalizeSlug(slug);

        if (sortOrder < 0)
        {
            throw new InvalidOperationException("Tenant category sort order cannot be negative.");
        }

        var category = new TenantCategory(
            Guid.NewGuid(),
            normalizedName,
            normalizedSlug,
            sortOrder);

        category.RaiseDomainEvent(new TenantCategoryCreatedDomainEvent(
            category.Id,
            category.Slug,
            category.Name));

        return category;
    }

    public static string NormalizeSlug(string slug)
    {
        string normalizedSlug = NormalizeRequired(slug, "Tenant category slug").ToLowerInvariant();

        if (normalizedSlug.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-'))
        {
            throw new InvalidOperationException("Tenant category slug can contain only letters, digits, and hyphens.");
        }

        return normalizedSlug;
    }

    public void Update(string name, string slug, int sortOrder)
    {
        string normalizedName = NormalizeRequired(name, "Tenant category name");
        string normalizedSlug = NormalizeSlug(slug);

        if (sortOrder < 0)
        {
            throw new InvalidOperationException("Tenant category sort order cannot be negative.");
        }

        Name = normalizedName;
        Slug = normalizedSlug;
        SortOrder = sortOrder;

        RaiseDomainEvent(new TenantCategoryUpdatedDomainEvent(Id, Slug, Name));
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
