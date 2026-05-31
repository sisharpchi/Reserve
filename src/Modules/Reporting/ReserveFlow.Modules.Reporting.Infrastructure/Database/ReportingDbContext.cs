using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Reporting.Application.Reports;
using ReserveFlow.Modules.Reporting.Domain.Reports;
using ReserveFlow.Modules.Reporting.Infrastructure.Reports;

namespace ReserveFlow.Modules.Reporting.Infrastructure.Database;

public sealed class ReportingDbContext(DbContextOptions<ReportingDbContext> options)
    : DbContext(options), IReportingUnitOfWork
{
    public DbSet<DailyBookingReport> DailyBookingReports => Set<DailyBookingReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Reporting);
        modelBuilder.ApplyConfiguration(new DailyBookingReportConfiguration());
    }
}
