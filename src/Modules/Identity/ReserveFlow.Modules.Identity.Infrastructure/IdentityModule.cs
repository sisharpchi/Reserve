using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ReserveFlow.Common.Application.Data;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Identity.Application.CurrentUser;
using ReserveFlow.Modules.Identity.Application.Keycloak;
using ReserveFlow.Modules.Identity.Application.TenantUsers;
using ReserveFlow.Modules.Identity.Application.TenantUsers.AssignTenantOwner;
using ReserveFlow.Modules.Identity.Application.TenantUsers.InviteTenantOwner;
using ReserveFlow.Modules.Identity.Application.Users;
using ReserveFlow.Modules.Identity.Application.Users.SyncUser;
using ReserveFlow.Modules.Identity.Infrastructure.CurrentUser;
using ReserveFlow.Modules.Identity.Infrastructure.Database;
using ReserveFlow.Modules.Identity.Infrastructure.Keycloak;
using ReserveFlow.Modules.Identity.Infrastructure.TenantUsers;
using ReserveFlow.Modules.Identity.Infrastructure.Users;

namespace ReserveFlow.Modules.Identity.Infrastructure;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantUserRepository, TenantUserRepository>();
        services.AddScoped<ICurrentUserPermissionReader, CurrentUserPermissionReader>();
        services.Configure<KeycloakAdminOptions>(configuration.GetSection(KeycloakAdminOptions.SectionName));
        services.AddHttpClient<IKeycloakAdminClient, KeycloakAdminClient>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IIdentityUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>, GetCurrentUserQueryHandler>();
        services.AddScoped<ICommandHandler<SyncUserCommand, SyncUserResponse>, SyncUserCommandHandler>();
        services.AddScoped<ICommandHandler<AssignTenantOwnerCommand, AssignTenantOwnerResponse>, AssignTenantOwnerCommandHandler>();
        services.AddScoped<ICommandHandler<InviteTenantOwnerCommand, InviteTenantOwnerResponse>, InviteTenantOwnerCommandHandler>();
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
