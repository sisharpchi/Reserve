using Microsoft.Extensions.Logging;

namespace ReserveFlow.Common.Infrastructure.Outbox;

public sealed class LoggingOutboxMessageDispatcher(
    ILogger<LoggingOutboxMessageDispatcher> logger) : IOutboxMessageDispatcher
{
    public Task DispatchAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        LogOutboxMessageDispatched(logger, message.Id, message.Type, message.TenantId, null);

        return Task.CompletedTask;
    }

    private static readonly Action<ILogger, Guid, string, Guid?, Exception?> LogOutboxMessageDispatched =
        LoggerMessage.Define<Guid, string, Guid?>(
            LogLevel.Information,
            new EventId(1, nameof(LogOutboxMessageDispatched)),
            "Outbox message {OutboxMessageId} with type {OutboxMessageType} and tenant {TenantId} was dispatched by the baseline dispatcher.");
}
