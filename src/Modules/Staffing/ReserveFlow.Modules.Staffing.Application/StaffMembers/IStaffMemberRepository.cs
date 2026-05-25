using ReserveFlow.Modules.Staffing.Domain.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers;

public interface IStaffMemberRepository
{
    void Insert(StaffMember staffMember);

    Task<IReadOnlyList<StaffMember>> GetActiveByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
