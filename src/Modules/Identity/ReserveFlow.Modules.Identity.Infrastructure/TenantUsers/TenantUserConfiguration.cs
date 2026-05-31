using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Identity.Domain.TenantUsers;

namespace ReserveFlow.Modules.Identity.Infrastructure.TenantUsers;

internal sealed class TenantUserConfiguration : IEntityTypeConfiguration<TenantUser>
{
    public void Configure(EntityTypeBuilder<TenantUser> builder)
    {
        builder.ToTable("tenant_users");

        builder.HasKey(tenantUser => tenantUser.Id);

        builder.Property(tenantUser => tenantUser.Id).HasColumnName("id");
        builder.Property(tenantUser => tenantUser.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(tenantUser => tenantUser.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(tenantUser => tenantUser.Role).HasColumnName("role").HasMaxLength(100).IsRequired();
        builder.Property(tenantUser => tenantUser.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(tenantUser => new { tenantUser.TenantId, tenantUser.UserId }).IsUnique();
        builder.HasIndex(tenantUser => tenantUser.UserId);
    }
}
