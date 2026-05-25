using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Application.Data;
using ReserveFlow.Modules.Identity.Domain.TenantUsers;
using ReserveFlow.Modules.Identity.Domain.Users;
using ReserveFlow.Modules.Identity.Infrastructure.TenantUsers;
using ReserveFlow.Modules.Identity.Infrastructure.Users;

namespace ReserveFlow.Modules.Identity.Infrastructure.Database;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Identity);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TenantUserConfiguration());
    }
}
