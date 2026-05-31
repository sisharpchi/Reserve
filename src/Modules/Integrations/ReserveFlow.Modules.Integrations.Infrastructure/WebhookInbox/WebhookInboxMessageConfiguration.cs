using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Integrations.Domain.WebhookInbox;

namespace ReserveFlow.Modules.Integrations.Infrastructure.WebhookInbox;

internal sealed class WebhookInboxMessageConfiguration : IEntityTypeConfiguration<WebhookInboxMessage>
{
    public void Configure(EntityTypeBuilder<WebhookInboxMessage> builder)
    {
        builder.ToTable("webhook_inbox_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id).HasColumnName("id");
        builder.Property(message => message.TenantId).HasColumnName("tenant_id");
        builder.Property(message => message.Source).HasColumnName("source").HasMaxLength(100).IsRequired();
        builder.Property(message => message.ExternalMessageId).HasColumnName("external_message_id").HasMaxLength(200).IsRequired();
        builder.Property(message => message.EventType).HasColumnName("event_type").HasMaxLength(200).IsRequired();
        builder.Property(message => message.PayloadJson).HasColumnName("payload_json").HasColumnType("jsonb").IsRequired();
        builder.Property(message => message.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(message => message.ReceivedAtUtc).HasColumnName("received_at_utc").IsRequired();
        builder.Property(message => message.ProcessedAtUtc).HasColumnName("processed_at_utc");
        builder.Property(message => message.Error).HasColumnName("error");

        builder.Ignore(message => message.DomainEvents);

        builder.HasIndex(message => new { message.Source, message.ExternalMessageId })
            .IsUnique()
            .HasDatabaseName("ux_webhook_inbox_messages_source_external_id");

        builder.HasIndex(message => new { message.TenantId, message.ReceivedAtUtc })
            .HasDatabaseName("ix_webhook_inbox_messages_tenant_received");
    }
}
