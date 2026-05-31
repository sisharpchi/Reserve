using ReserveFlow.Modules.Staffing.Domain.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers;

public interface IStaffMemberRepository
{
    void Insert(StaffMember staffMember);

    Task<StaffMember?> GetByIdAsync(Guid staffMemberId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StaffMember>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StaffMember>> GetActiveByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
