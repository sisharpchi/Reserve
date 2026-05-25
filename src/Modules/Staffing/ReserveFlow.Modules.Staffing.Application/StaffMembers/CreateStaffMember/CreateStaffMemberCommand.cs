using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;

namespace ReserveFlow.Modules.Staffing.Application.StaffMembers.CreateStaffMember;

public sealed record CreateStaffMemberCommand(
    Guid TenantId,
    string DisplayName,
    string? Email) : ICommand<StaffMemberResponse>;
