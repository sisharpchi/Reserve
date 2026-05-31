using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Identity.Domain.TenantUsers;

public sealed class TenantUser : Entity
{
    private TenantUser(
        Guid id,
        Guid tenantId,
        Guid userId,
        string role)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Role = NormalizeRequired(role, "Role");
        CreatedAtUtc = DateTime.UtcNow;
    }

    private TenantUser()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public string Role { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public static TenantUser Create(Guid tenantId, Guid userId, string role)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id is required.");
        }

        if (userId == Guid.Empty)
        {
            throw new InvalidOperationException("User id is required.");
        }

        return new TenantUser(Guid.NewGuid(), tenantId, userId, role);
    }

    public void AssignRole(string role)
    {
        Role = NormalizeRequired(role, "Role");
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
