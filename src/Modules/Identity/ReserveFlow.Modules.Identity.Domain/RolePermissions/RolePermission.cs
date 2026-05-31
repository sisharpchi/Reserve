using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Identity.Domain.RolePermissions;

public sealed class RolePermission : Entity
{
    private RolePermission(
        Guid id,
        Guid roleId,
        Guid permissionId)
        : base(id)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private RolePermission()
    {
    }

    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static RolePermission Grant(Guid roleId, Guid permissionId)
    {
        if (roleId == Guid.Empty)
        {
            throw new InvalidOperationException("Role id is required.");
        }

        if (permissionId == Guid.Empty)
        {
            throw new InvalidOperationException("Permission id is required.");
        }

        return new RolePermission(Guid.NewGuid(), roleId, permissionId);
    }
}
