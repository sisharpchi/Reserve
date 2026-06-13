namespace ReserveFlow.Modules.Identity.Presentation;

public sealed record InviteTenantOwnerRequest(
    string Email,
    string? DisplayName);
