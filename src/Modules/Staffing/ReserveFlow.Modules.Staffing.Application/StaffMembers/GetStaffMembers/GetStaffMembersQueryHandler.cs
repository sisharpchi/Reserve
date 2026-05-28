using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.GetStaffMembers;

public sealed class GetStaffMembersQueryHandler(IStaffMemberRepository staffMemberRepository)
    : IQueryHandler<GetStaffMembersQuery, IReadOnlyList<StaffMemberResponse>>
{
    public async Task<IReadOnlyList<StaffMemberResponse>> Handle(
        GetStaffMembersQuery query,
        CancellationToken cancellationToken = default)
    {
        var staffMembers = await staffMemberRepository.GetByTenantIdAsync(query.TenantId, cancellationToken);

        return staffMembers
            .Select(StaffMemberResponse.FromStaffMember)
            .ToArray();
    }
}
