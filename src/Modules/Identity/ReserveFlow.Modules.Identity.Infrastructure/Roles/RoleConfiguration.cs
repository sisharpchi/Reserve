using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Identity.Domain.Roles;

namespace ReserveFlow.Modules.Identity.Infrastructure.Roles;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id).HasColumnName("id");
        builder.Property(role => role.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(role => role.DisplayName).HasColumnName("display_name").HasMaxLength(200).IsRequired();
        builder.Property(role => role.IsPlatformRole).HasColumnName("is_platform_role").IsRequired();
        builder.Property(role => role.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(role => role.Name).IsUnique();
    }
}
