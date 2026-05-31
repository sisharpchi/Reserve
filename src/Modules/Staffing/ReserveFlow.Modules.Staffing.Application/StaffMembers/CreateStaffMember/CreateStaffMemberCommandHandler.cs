using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Staffing.Domain.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.CreateStaffMember;

public sealed class CreateStaffMemberCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IStaffingUnitOfWork unitOfWork) : ICommandHandler<CreateStaffMemberCommand, StaffMemberResponse>
{
    public async Task<StaffMemberResponse> Handle(
        CreateStaffMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        StaffMember staffMember = StaffMember.Create(
            command.TenantId,
            command.DisplayName,
            command.Email);

        staffMemberRepository.Insert(staffMember);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return StaffMemberResponse.FromStaffMember(staffMember);
    }
}
