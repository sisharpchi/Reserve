namespace ReserveFlow.Common.Application.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }

    Guid? StaffMemberId { get; }

    string? KeycloakSubject { get; }

    string? Email { get; }

    IReadOnlySet<string> Permissions { get; }
}
