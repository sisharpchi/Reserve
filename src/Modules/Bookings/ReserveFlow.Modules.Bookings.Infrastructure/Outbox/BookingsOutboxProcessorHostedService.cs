using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReserveFlow.Common.Infrastructure.Outbox;
using ReserveFlow.Modules.Bookings.Infrastructure.Database;

namespace ReserveFlow.Modules.Bookings.Infrastructure.Outbox;

internal sealed class BookingsOutboxProcessorHostedService(
    IServiceScopeFactory serviceScopeFactory,
    IOptionsMonitor<BookingsOutboxOptions> optionsMonitor,
    ILogger<BookingsOutboxProcessorHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!optionsMonitor.CurrentValue.Enabled)
        {
            LogOutboxProcessorDisabled(logger, null);
            return;
        }

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
            LogOutboxBatchFailed(logger, exception);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();

        BookingsDbContext dbContext = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        IOutboxMessageDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxMessageDispatcher>();
        int batchSize = optionsMonitor.CurrentValue.GetSafeBatchSize();

        OutboxMessage[] messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);

        foreach (OutboxMessage message in messages)
        {
            await ProcessMessageAsync(dispatcher, message, cancellationToken);
        }

        if (messages.Length > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task ProcessMessageAsync(
        IOutboxMessageDispatcher dispatcher,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            await dispatcher.DispatchAsync(message, cancellationToken);
            message.MarkProcessed(DateTime.UtcNow);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            message.RecordFailure(exception.Message);
        }
    }

    private static readonly Action<ILogger, Exception?> LogOutboxBatchFailed =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(1, nameof(LogOutboxBatchFailed)),
            "Bookings outbox batch processing failed. The worker will retry on the next polling interval.");

    private static readonly Action<ILogger, Exception?> LogOutboxProcessorDisabled =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, nameof(LogOutboxProcessorDisabled)),
            "Bookings outbox processor is disabled.");
}
