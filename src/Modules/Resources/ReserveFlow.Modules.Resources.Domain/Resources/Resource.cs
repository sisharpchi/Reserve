using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Resources.Domain.Resources;

public sealed class Resource : Entity
{
    private Resource(
        Guid id,
        Guid tenantId,
        string name,
        string resourceType,
        int capacity)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        ResourceType = resourceType;
        Capacity = capacity;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private Resource()
    {
    }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string ResourceType { get; private set; } = string.Empty;

    public int Capacity { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static Resource Create(
        Guid tenantId,
        string name,
        string resourceType,
        int capacity)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id is required.");
        }

        string normalizedName = NormalizeRequired(name, "Resource name");
        string normalizedResourceType = NormalizeRequired(resourceType, "Resource type").ToLowerInvariant();

        if (capacity <= 0)
        {
            throw new InvalidOperationException("Resource capacity must be greater than zero.");
        }

        var resource = new Resource(
            Guid.NewGuid(),
            tenantId,
            normalizedName,
            normalizedResourceType,
            capacity);

        resource.RaiseDomainEvent(new ResourceCreatedDomainEvent(
            resource.Id,
            resource.TenantId,
            resource.Name,
            resource.ResourceType));

        return resource;
    }

    public void Update(
        string name,
        string resourceType,
        int capacity)
    {
        Name = NormalizeRequired(name, "Resource name");
        ResourceType = NormalizeRequired(resourceType, "Resource type").ToLowerInvariant();

        if (capacity <= 0)
        {
            throw new InvalidOperationException("Resource capacity must be greater than zero.");
        }

        Capacity = capacity;

        RaiseDomainEvent(new ResourceUpdatedDomainEvent(Id, TenantId, Name, ResourceType));
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Resource is already inactive.");
        }

        IsActive = false;
        RaiseDomainEvent(new ResourceDeactivatedDomainEvent(Id, TenantId));
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
