using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.DeactivateStaffMember;

public sealed class DeactivateStaffMemberCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    ITenantAccessGuard tenantAccessGuard,
    IStaffingUnitOfWork unitOfWork) : ICommandHandler<DeactivateStaffMemberCommand, StaffMemberResponse?>
{
    public async Task<StaffMemberResponse?> Handle(
        DeactivateStaffMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        StaffMember? staffMember = await staffMemberRepository.GetByIdAsync(command.StaffMemberId, cancellationToken);

        if (staffMember is null || !tenantAccessGuard.CanAccessTenant(staffMember.TenantId))
        {
            return null;
        }

        staffMember.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return StaffMemberResponse.FromStaffMember(staffMember);
    }
}
