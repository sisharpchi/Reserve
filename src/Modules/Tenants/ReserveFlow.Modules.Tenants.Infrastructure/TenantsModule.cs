using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.TenantCategories;
using ReserveFlow.Modules.Tenants.Application.TenantCategories.CreateTenantCategory;
using ReserveFlow.Modules.Tenants.Application.TenantCategories.GetPlatformTenantCategories;
using ReserveFlow.Modules.Tenants.Application.TenantCategories.GetPublicTenantCategories;
using ReserveFlow.Modules.Tenants.Application.TenantCategories.UpdateTenantCategory;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Application.Tenants.ActivateTenant;
using ReserveFlow.Modules.Tenants.Application.Tenants.CreateTenant;
using ReserveFlow.Modules.Tenants.Application.Tenants.GetPlatformTenants;
using ReserveFlow.Modules.Tenants.Application.Tenants.GetPlatformUsage;
using ReserveFlow.Modules.Tenants.Application.Tenants.GetTenant;
using ReserveFlow.Modules.Tenants.Application.Tenants.GetTenantBySlug;
using ReserveFlow.Modules.Tenants.Application.Tenants.GetPublicTenants;
using ReserveFlow.Modules.Tenants.Application.Tenants.SuspendTenant;
using ReserveFlow.Modules.Tenants.Application.Tenants.UpdateTenant;
using ReserveFlow.Modules.Tenants.Infrastructure.Database;
using ReserveFlow.Modules.Tenants.Infrastructure.TenantCategories;
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

        services.AddScoped<ITenantCategoryRepository, TenantCategoryRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantsUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<TenantsDbContext>());
        services.AddScoped<ICommandHandler<CreateTenantCategoryCommand, TenantCategoryResponse>, CreateTenantCategoryCommandHandler>();
        services.AddScoped<IQueryHandler<GetPlatformTenantCategoriesQuery, IReadOnlyList<TenantCategoryResponse>>, GetPlatformTenantCategoriesQueryHandler>();
        services.AddScoped<IQueryHandler<GetPublicTenantCategoriesQuery, IReadOnlyList<TenantCategoryResponse>>, GetPublicTenantCategoriesQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateTenantCategoryCommand, TenantCategoryResponse?>, UpdateTenantCategoryCommandHandler>();
        services.AddScoped<ICommandHandler<CreateTenantCommand, TenantResponse>, CreateTenantCommandHandler>();
        services.AddScoped<ICommandHandler<ActivateTenantCommand, TenantResponse?>, ActivateTenantCommandHandler>();
        services.AddScoped<IQueryHandler<GetPlatformTenantsQuery, IReadOnlyList<TenantResponse>>, GetPlatformTenantsQueryHandler>();
        services.AddScoped<IQueryHandler<GetPlatformUsageQuery, PlatformUsageResponse>, GetPlatformUsageQueryHandler>();
        services.AddScoped<IQueryHandler<GetTenantQuery, TenantResponse?>, GetTenantQueryHandler>();
        services.AddScoped<IQueryHandler<GetTenantBySlugQuery, TenantResponse?>, GetTenantBySlugQueryHandler>();
        services.AddScoped<IQueryHandler<GetPublicTenantsQuery, IReadOnlyList<TenantResponse>>, GetPublicTenantsQueryHandler>();
        services.AddScoped<ICommandHandler<SuspendTenantCommand, TenantResponse?>, SuspendTenantCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateTenantCommand, TenantResponse?>, UpdateTenantCommandHandler>();
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
