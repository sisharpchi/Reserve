using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Identity.Domain.UserRoles;

namespace ReserveFlow.Modules.Identity.Infrastructure.UserRoles;

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(userRole => userRole.Id);

        builder.Property(userRole => userRole.Id).HasColumnName("id");
        builder.Property(userRole => userRole.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(userRole => userRole.RoleId).HasColumnName("role_id").IsRequired();
        builder.Property(userRole => userRole.TenantId).HasColumnName("tenant_id");
        builder.Property(userRole => userRole.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(userRole => new { userRole.UserId, userRole.RoleId, userRole.TenantId }).IsUnique();
        builder.HasIndex(userRole => new { userRole.TenantId, userRole.RoleId });
    }
}
