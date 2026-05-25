using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.GetActiveStaffMembers;

public sealed class GetActiveStaffMembersQueryHandler(IStaffMemberRepository staffMemberRepository)
    : IQueryHandler<GetActiveStaffMembersQuery, IReadOnlyList<StaffMemberResponse>>
{
    public async Task<IReadOnlyList<StaffMemberResponse>> Handle(
        GetActiveStaffMembersQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<StaffMember> staffMembers = await staffMemberRepository.GetActiveByTenantIdAsync(
            query.TenantId,
            cancellationToken);

        return staffMembers
            .Select(StaffMemberResponse.FromStaffMember)
            .ToArray();
    }
}
