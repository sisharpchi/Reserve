using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Domain.Resources;
using ReserveFlow.Modules.Resources.Infrastructure.Resources;

namespace ReserveFlow.Modules.Resources.Infrastructure.Database;

public sealed class ResourcesDbContext(DbContextOptions<ResourcesDbContext> options)
    : DbContext(options), IResourcesUnitOfWork
{
    public DbSet<Resource> Resources => Set<Resource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Resources);
        modelBuilder.ApplyConfiguration(new ResourceConfiguration());
    }
}
