using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Audit.Domain.AuditLogs;

namespace ReserveFlow.Modules.Audit.Infrastructure.AuditLogs;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.Id).HasColumnName("id");
        builder.Property(log => log.TenantId).HasColumnName("tenant_id");
        builder.Property(log => log.UserId).HasColumnName("user_id");
        builder.Property(log => log.Action).HasColumnName("action").HasMaxLength(200).IsRequired();
        builder.Property(log => log.EntityName).HasColumnName("entity_name").HasMaxLength(200).IsRequired();
        builder.Property(log => log.EntityId).HasColumnName("entity_id");
        builder.Property(log => log.DetailsJson).HasColumnName("details_json").HasColumnType("jsonb").IsRequired();
        builder.Property(log => log.OccurredOnUtc).HasColumnName("occurred_on_utc").IsRequired();

        builder.Ignore(log => log.DomainEvents);

        builder.HasIndex(log => new { log.TenantId, log.OccurredOnUtc })
            .HasDatabaseName("ix_audit_logs_tenant_occurred");
    }
}
