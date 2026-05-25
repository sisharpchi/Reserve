using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;
using ReserveFlow.Modules.Staffing.Infrastructure.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Infrastructure.Database;

public sealed class StaffingDbContext(DbContextOptions<StaffingDbContext> options)
    : DbContext(options), IStaffingUnitOfWork
{
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Staffing);
        modelBuilder.ApplyConfiguration(new StaffMemberConfiguration());
    }
}
