using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Infrastructure.StaffMembers;

internal sealed class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
{
    public void Configure(EntityTypeBuilder<StaffMember> builder)
    {
        builder.ToTable("staff_members");

        builder.HasKey(staffMember => staffMember.Id);

        builder.Property(staffMember => staffMember.Id).HasColumnName("id");
        builder.Property(staffMember => staffMember.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(staffMember => staffMember.DisplayName).HasColumnName("display_name").HasMaxLength(200).IsRequired();
        builder.Property(staffMember => staffMember.Email).HasColumnName("email").HasMaxLength(320);
        builder.Property(staffMember => staffMember.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(staffMember => staffMember.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.Ignore(staffMember => staffMember.DomainEvents);

        builder.HasIndex(staffMember => new { staffMember.TenantId, staffMember.IsActive })
            .HasDatabaseName("ix_staff_members_tenant_active");

        builder.HasIndex(staffMember => new { staffMember.TenantId, staffMember.Email })
            .HasDatabaseName("ix_staff_members_tenant_email");
    }
}
