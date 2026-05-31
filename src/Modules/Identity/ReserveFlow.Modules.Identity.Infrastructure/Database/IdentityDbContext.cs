using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Application.Data;
using ReserveFlow.Modules.Identity.Application.Users;
using ReserveFlow.Modules.Identity.Domain.Permissions;
using ReserveFlow.Modules.Identity.Domain.RolePermissions;
using ReserveFlow.Modules.Identity.Domain.Roles;
using ReserveFlow.Modules.Identity.Domain.TenantUsers;
using ReserveFlow.Modules.Identity.Domain.UserRoles;
using ReserveFlow.Modules.Identity.Domain.Users;
using ReserveFlow.Modules.Identity.Infrastructure.Permissions;
using ReserveFlow.Modules.Identity.Infrastructure.RolePermissions;
using ReserveFlow.Modules.Identity.Infrastructure.Roles;
using ReserveFlow.Modules.Identity.Infrastructure.TenantUsers;
using ReserveFlow.Modules.Identity.Infrastructure.UserRoles;
using ReserveFlow.Modules.Identity.Infrastructure.Users;

namespace ReserveFlow.Modules.Identity.Infrastructure.Database;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : DbContext(options), IUnitOfWork, IIdentityUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Identity);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TenantUserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
    }
}
