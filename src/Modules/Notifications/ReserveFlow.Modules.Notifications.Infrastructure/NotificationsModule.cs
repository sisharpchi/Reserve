using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Notifications.Application.Notifications;
using ReserveFlow.Modules.Notifications.Application.Notifications.GetNotifications;
using ReserveFlow.Modules.Notifications.Application.Notifications.QueueNotification;
using ReserveFlow.Modules.Notifications.Infrastructure.Database;
using ReserveFlow.Modules.Notifications.Infrastructure.Delivery;
using ReserveFlow.Modules.Notifications.Infrastructure.Notifications;
using ReserveFlow.Modules.Notifications.Infrastructure.Sending;

namespace ReserveFlow.Modules.Notifications.Infrastructure;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Notifications)));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationsUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<NotificationsDbContext>());
        services.AddScoped<INotificationSender, FakeNotificationSender>();
        services.AddScoped<ICommandHandler<QueueNotificationCommand, NotificationResponse>, QueueNotificationCommandHandler>();
        services.AddScoped<IQueryHandler<GetNotificationsQuery, IReadOnlyList<NotificationResponse>>, GetNotificationsQueryHandler>();
        services.Configure<NotificationDeliveryOptions>(configuration.GetSection("Notifications:Delivery"));
        services.AddHostedService<NotificationDeliveryHostedService>();
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
