using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Audit.Application.AuditLogs;
using ReserveFlow.Modules.Audit.Domain.AuditLogs;
using ReserveFlow.Modules.Audit.Infrastructure.AuditLogs;

namespace ReserveFlow.Modules.Audit.Infrastructure.Database;

public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options)
    : DbContext(options), IAuditUnitOfWork
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Audit);
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
    }
}
