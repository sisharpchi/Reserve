namespace ReserveFlow.Modules.Staffing.Presentation;

internal sealed record CreateStaffMemberRequest(
    Guid TenantId,
    string DisplayName,
    string? Email);
