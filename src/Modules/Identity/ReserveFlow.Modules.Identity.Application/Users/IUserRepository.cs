using ReserveFlow.Modules.Identity.Domain.Users;

namespace ReserveFlow.Modules.Identity.Application.Users;

public interface IUserRepository
{
    void Insert(User user);

    Task<User?> GetByKeycloakSubjectAsync(
        string keycloakSubject,
        CancellationToken cancellationToken = default);
}
