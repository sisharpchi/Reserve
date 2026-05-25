namespace ReserveFlow.Modules.Staffing.Application.StaffMembers;

public interface IStaffingUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
