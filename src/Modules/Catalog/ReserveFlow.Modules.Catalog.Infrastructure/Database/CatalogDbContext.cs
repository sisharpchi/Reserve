using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Domain.Services;
using ReserveFlow.Modules.Catalog.Infrastructure.Services;

namespace ReserveFlow.Modules.Catalog.Infrastructure.Database;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options), ICatalogUnitOfWork
{
    public DbSet<Service> Services => Set<Service>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Catalog);
        modelBuilder.ApplyConfiguration(new ServiceConfiguration());
    }
}
