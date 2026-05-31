using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Identity.Domain.UserRoles;

public sealed class UserRole : Entity
{
    private UserRole(
        Guid id,
        Guid userId,
        Guid roleId,
        Guid? tenantId)
        : base(id)
    {
        UserId = userId;
        RoleId = roleId;
        TenantId = tenantId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private UserRole()
    {
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    public Guid? TenantId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static UserRole Assign(Guid userId, Guid roleId, Guid? tenantId)
    {
        if (userId == Guid.Empty)
        {
            throw new InvalidOperationException("User id is required.");
        }

        if (roleId == Guid.Empty)
        {
            throw new InvalidOperationException("Role id is required.");
        }

        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id cannot be empty.");
        }

        return new UserRole(Guid.NewGuid(), userId, roleId, tenantId);
    }
}
