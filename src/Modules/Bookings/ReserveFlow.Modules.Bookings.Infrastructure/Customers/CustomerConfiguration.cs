using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Bookings.Domain.Customers;

namespace ReserveFlow.Modules.Bookings.Infrastructure.Customers;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Id).HasColumnName("id");
        builder.Property(customer => customer.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(customer => customer.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        builder.Property(customer => customer.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(customer => customer.PhoneNumber).HasColumnName("phone_number").HasMaxLength(50);
        builder.Property(customer => customer.RegisteredAtUtc).HasColumnName("registered_at_utc").IsRequired();

        builder.Ignore(customer => customer.DomainEvents);

        builder.HasIndex(customer => new { customer.TenantId, customer.Email })
            .IsUnique()
            .HasDatabaseName("ux_customers_tenant_email");
    }
}
