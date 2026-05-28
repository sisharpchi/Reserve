using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.GetStaffMember;

public sealed class GetStaffMemberQueryHandler(
    IStaffMemberRepository staffMemberRepository,
    ITenantAccessGuard tenantAccessGuard)
    : IQueryHandler<GetStaffMemberQuery, StaffMemberResponse?>
{
    public async Task<StaffMemberResponse?> Handle(
        GetStaffMemberQuery query,
        CancellationToken cancellationToken = default)
    {
        StaffMember? staffMember = await staffMemberRepository.GetByIdAsync(query.StaffMemberId, cancellationToken);

        if (staffMember is null || !tenantAccessGuard.CanAccessTenant(staffMember.TenantId))
        {
            return null;
        }

        return StaffMemberResponse.FromStaffMember(staffMember);
    }
}
