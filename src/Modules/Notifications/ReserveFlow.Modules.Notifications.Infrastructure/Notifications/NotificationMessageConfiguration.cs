using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Notifications.Infrastructure.Notifications;

internal sealed class NotificationMessageConfiguration : IEntityTypeConfiguration<NotificationMessage>
{
    public void Configure(EntityTypeBuilder<NotificationMessage> builder)
    {
        builder.ToTable("notification_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id).HasColumnName("id");
        builder.Property(message => message.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(message => message.Channel).HasColumnName("channel").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(message => message.Recipient).HasColumnName("recipient").HasMaxLength(320).IsRequired();
        builder.Property(message => message.Subject).HasColumnName("subject").HasMaxLength(300).IsRequired();
        builder.Property(message => message.Body).HasColumnName("body").IsRequired();
        builder.Property(message => message.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(message => message.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(message => message.DeliverAtUtc).HasColumnName("deliver_at_utc").IsRequired();
        builder.Property(message => message.SentAtUtc).HasColumnName("sent_at_utc");
        builder.Property(message => message.Error).HasColumnName("error");

        builder.Ignore(message => message.DomainEvents);

        builder.HasIndex(message => new { message.TenantId, message.Status, message.CreatedAtUtc })
            .HasDatabaseName("ix_notification_messages_tenant_status_created");

        builder.HasIndex(message => new { message.Status, message.DeliverAtUtc })
            .HasDatabaseName("ix_notification_messages_status_deliver_at");
    }
}
