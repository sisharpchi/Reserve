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
        Role = role;
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

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new InvalidOperationException("Role is required.");
        }

        return new TenantUser(Guid.NewGuid(), tenantId, userId, role);
    }
}
