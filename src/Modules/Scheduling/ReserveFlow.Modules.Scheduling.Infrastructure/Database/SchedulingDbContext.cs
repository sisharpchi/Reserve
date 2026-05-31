using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours;
using ReserveFlow.Modules.Scheduling.Domain.UnavailablePeriods;
using ReserveFlow.Modules.Scheduling.Domain.WorkingHours;
using ReserveFlow.Modules.Scheduling.Infrastructure.UnavailablePeriods;
using ReserveFlow.Modules.Scheduling.Infrastructure.WorkingHours;

namespace ReserveFlow.Modules.Scheduling.Infrastructure.Database;

public sealed class SchedulingDbContext(DbContextOptions<SchedulingDbContext> options)
    : DbContext(options), ISchedulingUnitOfWork
{
    public DbSet<UnavailablePeriod> UnavailablePeriods => Set<UnavailablePeriod>();

    public DbSet<WorkingHour> WorkingHours => Set<WorkingHour>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Scheduling);
        modelBuilder.ApplyConfiguration(new UnavailablePeriodConfiguration());
        modelBuilder.ApplyConfiguration(new WorkingHourConfiguration());
    }
}
