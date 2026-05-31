namespace ReserveFlow.Modules.Staffing.Presentation;

internal sealed record UpdateStaffMemberRequest(
    string DisplayName,
    string? Email);
