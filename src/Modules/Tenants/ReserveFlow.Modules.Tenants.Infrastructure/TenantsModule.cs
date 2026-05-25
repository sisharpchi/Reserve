using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Application.Tenants.CreateTenant;
using ReserveFlow.Modules.Tenants.Application.Tenants.GetTenantBySlug;
using ReserveFlow.Modules.Tenants.Infrastructure.Database;
using ReserveFlow.Modules.Tenants.Infrastructure.Tenants;

namespace ReserveFlow.Modules.Tenants.Infrastructure;

public static class TenantsModule
{
    public static IServiceCollection AddTenantsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TenantsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Platform)));

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantsUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<TenantsDbContext>());
        services.AddScoped<ICommandHandler<CreateTenantCommand, TenantResponse>, CreateTenantCommandHandler>();
        services.AddScoped<IQueryHandler<GetTenantBySlugQuery, TenantResponse?>, GetTenantBySlugQueryHandler>();
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
