using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Identity.Domain.RolePermissions;

namespace ReserveFlow.Modules.Identity.Infrastructure.RolePermissions;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(rolePermission => rolePermission.Id);

        builder.Property(rolePermission => rolePermission.Id).HasColumnName("id");
        builder.Property(rolePermission => rolePermission.RoleId).HasColumnName("role_id").IsRequired();
        builder.Property(rolePermission => rolePermission.PermissionId).HasColumnName("permission_id").IsRequired();
        builder.Property(rolePermission => rolePermission.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId }).IsUnique();
        builder.HasIndex(rolePermission => rolePermission.PermissionId);
    }
}
