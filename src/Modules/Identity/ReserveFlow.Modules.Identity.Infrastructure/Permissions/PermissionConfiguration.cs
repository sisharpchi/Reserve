using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Identity.Domain.Permissions;

namespace ReserveFlow.Modules.Identity.Infrastructure.Permissions;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Id).HasColumnName("id");
        builder.Property(permission => permission.Code).HasColumnName("code").HasMaxLength(150).IsRequired();
        builder.Property(permission => permission.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(permission => permission.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(permission => permission.Code).IsUnique();
    }
}
