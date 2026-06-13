using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Application.Resources.CreateResource;
using ReserveFlow.Modules.Resources.Application.Resources.DeactivateResource;
using ReserveFlow.Modules.Resources.Application.Resources.GetActiveResources;
using ReserveFlow.Modules.Resources.Application.Resources.GetResource;
using ReserveFlow.Modules.Resources.Application.Resources.GetResources;
using ReserveFlow.Modules.Resources.Application.Resources.UpdateResource;
using ReserveFlow.Modules.Resources.Infrastructure.Database;
using ReserveFlow.Modules.Resources.Infrastructure.Resources;

namespace ReserveFlow.Modules.Resources.Infrastructure;

public static class ResourcesModule
{
    public static IServiceCollection AddResourcesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ResourcesDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Resources)));

        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IResourcesUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ResourcesDbContext>());
        services.AddScoped<ICommandHandler<CreateResourceCommand, ResourceResponse>, CreateResourceCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateResourceCommand, ResourceResponse?>, UpdateResourceCommandHandler>();
        services.AddScoped<ICommandHandler<DeactivateResourceCommand, ResourceResponse?>, DeactivateResourceCommandHandler>();
        services.AddScoped<IQueryHandler<GetActiveResourcesQuery, IReadOnlyList<ResourceResponse>>, GetActiveResourcesQueryHandler>();
        services.AddScoped<IQueryHandler<GetResourcesQuery, PagedResponse<ResourceResponse>>, GetResourcesQueryHandler>();
        services.AddScoped<IQueryHandler<GetResourceQuery, ResourceResponse?>, GetResourceQueryHandler>();
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
