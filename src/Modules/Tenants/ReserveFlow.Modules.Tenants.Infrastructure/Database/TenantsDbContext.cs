using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Domain.Tenants;
using ReserveFlow.Modules.Tenants.Infrastructure.Tenants;

namespace ReserveFlow.Modules.Tenants.Infrastructure.Database;

public sealed class TenantsDbContext(DbContextOptions<TenantsDbContext> options)
    : DbContext(options), ITenantsUnitOfWork
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Platform);
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
    }
}
