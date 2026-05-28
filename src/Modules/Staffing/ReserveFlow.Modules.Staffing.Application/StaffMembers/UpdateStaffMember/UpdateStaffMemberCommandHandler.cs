using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.UpdateStaffMember;

public sealed class UpdateStaffMemberCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IStaffingUnitOfWork unitOfWork) : ICommandHandler<UpdateStaffMemberCommand, StaffMemberResponse?>
{
    public async Task<StaffMemberResponse?> Handle(
        UpdateStaffMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        StaffMember? staffMember = await staffMemberRepository.GetByIdAsync(command.StaffMemberId, cancellationToken);

        if (staffMember is null)
        {
            return null;
        }

        staffMember.Update(command.DisplayName, command.Email);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return StaffMemberResponse.FromStaffMember(staffMember);
    }
}
