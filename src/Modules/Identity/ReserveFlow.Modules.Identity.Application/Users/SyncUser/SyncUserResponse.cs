namespace ReserveFlow.Modules.Identity.Application.Users.SyncUser;

public sealed record SyncUserResponse(
    Guid UserId,
    string KeycloakSubject,
    string Email,
    string DisplayName,
    bool Created);
