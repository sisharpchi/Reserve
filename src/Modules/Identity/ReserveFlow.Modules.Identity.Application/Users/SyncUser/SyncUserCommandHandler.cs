using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Identity.Domain.Users;

namespace ReserveFlow.Modules.Identity.Application.Users.SyncUser;

public sealed class SyncUserCommandHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IIdentityUnitOfWork unitOfWork)
    : ICommandHandler<SyncUserCommand, SyncUserResponse>
{
    public async Task<SyncUserResponse> Handle(
        SyncUserCommand command,
        CancellationToken cancellationToken = default)
    {
        string keycloakSubject = NormalizeRequired(
            currentUser.KeycloakSubject,
            "Authenticated Keycloak subject");
        string email = NormalizeRequired(currentUser.Email, "Email");
        string displayName = email;

        User? user = await userRepository.GetByKeycloakSubjectAsync(keycloakSubject, cancellationToken);
        bool created = user is null;

        if (user is null)
        {
            user = User.Create(keycloakSubject, email, displayName);
            userRepository.Insert(user);
        }
        else
        {
            user.UpdateProfile(email, displayName);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SyncUserResponse(
            user.Id,
            user.KeycloakSubject,
            user.Email,
            user.DisplayName,
            created);
    }

    private static string NormalizeRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }
}
