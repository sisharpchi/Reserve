using ReserveFlow.Modules.Identity.Application.Keycloak;
using ReserveFlow.Modules.Identity.Application.TenantUsers;
using ReserveFlow.Modules.Identity.Application.TenantUsers.InviteTenantOwner;
using ReserveFlow.Modules.Identity.Application.Users;
using ReserveFlow.Modules.Identity.Domain.TenantUsers;
using ReserveFlow.Modules.Identity.Domain.Users;

namespace ReserveFlow.UnitTests.Identity;

public sealed class InviteTenantOwnerCommandHandlerTests
{
    [Fact]
    public async Task InviteTenantOwnerCreatesKeycloakUserAndLocalTenantAdminMembership()
    {
        var keycloakClient = new FakeKeycloakAdminClient(
            new KeycloakProvisionedUser(
                "keycloak-owner-1",
                "owner@example.com",
                "Clinic Owner",
                CreatedInKeycloak: true,
                InvitationEmailSent: true));
        var userRepository = new FakeUserRepository();
        var tenantUserRepository = new FakeTenantUserRepository();
        var unitOfWork = new FakeIdentityUnitOfWork();
        var handler = new InviteTenantOwnerCommandHandler(
            keycloakClient,
            userRepository,
            tenantUserRepository,
            unitOfWork);
        Guid tenantId = Guid.NewGuid();

        InviteTenantOwnerResponse response = await handler.Handle(
            new InviteTenantOwnerCommand(
                tenantId,
                " OWNER@EXAMPLE.COM ",
                " Clinic Owner "));

        Assert.Equal(tenantId, response.TenantId);
        Assert.Equal("keycloak-owner-1", response.KeycloakSubject);
        Assert.Equal("owner@example.com", response.Email);
        Assert.Equal("Clinic Owner", response.DisplayName);
        Assert.Equal("TenantAdmin", response.Role);
        Assert.True(response.CreatedInKeycloak);
        Assert.True(response.InvitationEmailSent);
        Assert.True(response.LocalUserCreated);
        Assert.True(response.MembershipCreated);
        Assert.Single(userRepository.Users);
        Assert.Single(tenantUserRepository.TenantUsers);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(("owner@example.com", "Clinic Owner"), Assert.Single(keycloakClient.Requests));
    }

    [Fact]
    public async Task InviteTenantOwnerReusesExistingLocalUserAndMembership()
    {
        var existingUser = User.Create(
            "keycloak-owner-2",
            "old@example.com",
            "Old Owner");
        var existingMembership = TenantUser.Create(
            Guid.NewGuid(),
            existingUser.Id,
            "Staff");
        var keycloakClient = new FakeKeycloakAdminClient(
            new KeycloakProvisionedUser(
                existingUser.KeycloakSubject,
                "owner2@example.com",
                "Updated Owner",
                CreatedInKeycloak: false,
                InvitationEmailSent: false));
        var userRepository = new FakeUserRepository(existingUser);
        var tenantUserRepository = new FakeTenantUserRepository(existingMembership);
        var unitOfWork = new FakeIdentityUnitOfWork();
        var handler = new InviteTenantOwnerCommandHandler(
            keycloakClient,
            userRepository,
            tenantUserRepository,
            unitOfWork);

        InviteTenantOwnerResponse response = await handler.Handle(
            new InviteTenantOwnerCommand(
                existingMembership.TenantId,
                "owner2@example.com",
                "Updated Owner"));

        Assert.False(response.CreatedInKeycloak);
        Assert.False(response.InvitationEmailSent);
        Assert.False(response.LocalUserCreated);
        Assert.False(response.MembershipCreated);
        Assert.Equal("owner2@example.com", existingUser.Email);
        Assert.Equal("Updated Owner", existingUser.DisplayName);
        Assert.Equal("TenantAdmin", existingMembership.Role);
        Assert.Single(userRepository.Users);
        Assert.Single(tenantUserRepository.TenantUsers);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private sealed class FakeKeycloakAdminClient(KeycloakProvisionedUser result) : IKeycloakAdminClient
    {
        public List<(string Email, string? DisplayName)> Requests { get; } = [];

        public Task<KeycloakProvisionedUser> ProvisionTenantAdminAsync(
            string email,
            string? displayName,
            CancellationToken cancellationToken = default)
        {
            Requests.Add((email, displayName));

            return Task.FromResult(result);
        }
    }

    private sealed class FakeUserRepository(params User[] users) : IUserRepository
    {
        public List<User> Users { get; } = [.. users];

        public void Insert(User user)
        {
            Users.Add(user);
        }

        public Task<User?> GetByKeycloakSubjectAsync(
            string keycloakSubject,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Users.FirstOrDefault(
                user => user.KeycloakSubject == keycloakSubject));
        }
    }

    private sealed class FakeTenantUserRepository(params TenantUser[] tenantUsers) : ITenantUserRepository
    {
        public List<TenantUser> TenantUsers { get; } = [.. tenantUsers];

        public void Insert(TenantUser tenantUser)
        {
            TenantUsers.Add(tenantUser);
        }

        public Task<TenantUser?> GetByTenantAndUserIdAsync(
            Guid tenantId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TenantUsers.FirstOrDefault(
                tenantUser => tenantUser.TenantId == tenantId && tenantUser.UserId == userId));
        }
    }

    private sealed class FakeIdentityUnitOfWork : IIdentityUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;

            return Task.FromResult(1);
        }
    }
}
