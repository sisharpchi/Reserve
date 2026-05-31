using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Integrations.Application.WebhookInbox;
using ReserveFlow.Modules.Integrations.Application.WebhookInbox.AcceptWebhook;
using ReserveFlow.Modules.Integrations.Infrastructure.Database;
using ReserveFlow.Modules.Integrations.Infrastructure.WebhookInbox;

namespace ReserveFlow.Modules.Integrations.Infrastructure;

public static class IntegrationsModule
{
    public static IServiceCollection AddIntegrationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IntegrationsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Integrations)));

        services.AddScoped<IWebhookInboxRepository, WebhookInboxRepository>();
        services.AddScoped<IIntegrationsUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<IntegrationsDbContext>());
        services.AddScoped<ICommandHandler<AcceptWebhookCommand, WebhookInboxMessageResponse>, AcceptWebhookCommandHandler>();
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
