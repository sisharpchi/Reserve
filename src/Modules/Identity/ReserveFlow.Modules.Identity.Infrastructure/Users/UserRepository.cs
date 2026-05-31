using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Identity.Application.Users;
using ReserveFlow.Modules.Identity.Domain.Users;
using ReserveFlow.Modules.Identity.Infrastructure.Database;

namespace ReserveFlow.Modules.Identity.Infrastructure.Users;

internal sealed class UserRepository(IdentityDbContext dbContext) : IUserRepository
{
    public void Insert(User user)
    {
        dbContext.Users.Add(user);
    }

    public async Task<User?> GetByKeycloakSubjectAsync(
        string keycloakSubject,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.FirstOrDefaultAsync(
            user => user.KeycloakSubject == keycloakSubject,
            cancellationToken);
    }
}
