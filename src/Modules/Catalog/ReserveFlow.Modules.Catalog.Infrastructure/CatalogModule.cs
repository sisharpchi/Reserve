using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Application.Services.CreateService;
using ReserveFlow.Modules.Catalog.Application.Services.DeactivateService;
using ReserveFlow.Modules.Catalog.Application.Services.GetActiveService;
using ReserveFlow.Modules.Catalog.Application.Services.GetActiveServices;
using ReserveFlow.Modules.Catalog.Application.Services.GetService;
using ReserveFlow.Modules.Catalog.Application.Services.GetServices;
using ReserveFlow.Modules.Catalog.Application.Services.UpdateService;
using ReserveFlow.Modules.Catalog.Infrastructure.Database;
using ReserveFlow.Modules.Catalog.Infrastructure.Services;

namespace ReserveFlow.Modules.Catalog.Infrastructure;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Catalog)));

        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<ICatalogUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<CatalogDbContext>());
        services.AddScoped<ICommandHandler<CreateServiceCommand, ServiceResponse>, CreateServiceCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateServiceCommand, ServiceResponse?>, UpdateServiceCommandHandler>();
        services.AddScoped<ICommandHandler<DeactivateServiceCommand, ServiceResponse?>, DeactivateServiceCommandHandler>();
        services.AddScoped<IQueryHandler<GetActiveServiceQuery, ServiceResponse?>, GetActiveServiceQueryHandler>();
        services.AddScoped<IQueryHandler<GetActiveServicesQuery, IReadOnlyList<ServiceResponse>>, GetActiveServicesQueryHandler>();
        services.AddScoped<IQueryHandler<GetServicesQuery, IReadOnlyList<ServiceResponse>>, GetServicesQueryHandler>();
        services.AddScoped<IQueryHandler<GetServiceQuery, ServiceResponse?>, GetServiceQueryHandler>();
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
