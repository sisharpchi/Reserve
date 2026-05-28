using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.GetStaffMember;

public sealed class GetStaffMemberQueryHandler(IStaffMemberRepository staffMemberRepository)
    : IQueryHandler<GetStaffMemberQuery, StaffMemberResponse?>
{
    public async Task<StaffMemberResponse?> Handle(
        GetStaffMemberQuery query,
        CancellationToken cancellationToken = default)
    {
        StaffMember? staffMember = await staffMemberRepository.GetByIdAsync(query.StaffMemberId, cancellationToken);

        return staffMember is null ? null : StaffMemberResponse.FromStaffMember(staffMember);
    }
}
