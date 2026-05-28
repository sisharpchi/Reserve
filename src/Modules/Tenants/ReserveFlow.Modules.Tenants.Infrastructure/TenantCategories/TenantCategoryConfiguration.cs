using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Modules.Tenants.Domain.TenantCategories;

namespace ReserveFlow.Modules.Tenants.Infrastructure.TenantCategories;

internal sealed class TenantCategoryConfiguration : IEntityTypeConfiguration<TenantCategory>
{
    public void Configure(EntityTypeBuilder<TenantCategory> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id).HasColumnName("id");
        builder.Property(category => category.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(category => category.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
        builder.Property(category => category.SortOrder).HasColumnName("sort_order").IsRequired();
        builder.Property(category => category.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(category => category.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.Ignore(category => category.DomainEvents);

        builder.HasIndex(category => category.Slug)
            .IsUnique()
            .HasDatabaseName("ux_categories_slug");

        builder.HasIndex(category => new { category.IsActive, category.SortOrder })
            .HasDatabaseName("ix_categories_active_sort");
    }
}
