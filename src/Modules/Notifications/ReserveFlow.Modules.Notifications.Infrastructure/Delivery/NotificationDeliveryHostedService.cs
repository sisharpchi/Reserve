using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReserveFlow.Modules.Notifications.Application.Notifications;
using ReserveFlow.Modules.Notifications.Domain.Notifications;
using ReserveFlow.Modules.Notifications.Infrastructure.Database;

namespace ReserveFlow.Modules.Notifications.Infrastructure.Delivery;

internal sealed class NotificationDeliveryHostedService(
    IServiceScopeFactory serviceScopeFactory,
    IOptionsMonitor<NotificationDeliveryOptions> optionsMonitor,
    ILogger<NotificationDeliveryHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await TryProcessBatchAsync(stoppingToken);

        using var timer = new PeriodicTimer(optionsMonitor.CurrentValue.GetSafePollingInterval());

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await TryProcessBatchAsync(stoppingToken);
        }
    }

    private async Task TryProcessBatchAsync(CancellationToken cancellationToken)
    {
        try
        {
            await ProcessBatchAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            LogNotificationBatchFailed(logger, exception);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();

        NotificationsDbContext dbContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        INotificationSender sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();
        int batchSize = optionsMonitor.CurrentValue.GetSafeBatchSize();
        DateTime nowUtc = DateTime.UtcNow;

        NotificationMessage[] messages = await dbContext.NotificationMessages
            .Where(message => message.Status == NotificationStatus.Pending && message.DeliverAtUtc <= nowUtc)
            .OrderBy(message => message.DeliverAtUtc)
            .ThenBy(message => message.CreatedAtUtc)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);

        foreach (NotificationMessage message in messages)
        {
            await ProcessMessageAsync(sender, message, cancellationToken);
        }

        if (messages.Length > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task ProcessMessageAsync(
        INotificationSender sender,
        NotificationMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            await sender.SendAsync(
                new NotificationDeliveryMessage(
                    message.Id,
                    message.Channel.ToString(),
                    message.Recipient,
                    message.Subject,
                    message.Body),
                cancellationToken);

            message.MarkSent(DateTime.UtcNow);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            message.MarkFailed(exception.Message);
        }
    }

    private static readonly Action<ILogger, Exception?> LogNotificationBatchFailed =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(1, nameof(LogNotificationBatchFailed)),
            "Notification delivery batch processing failed. The worker will retry on the next polling interval.");
}
