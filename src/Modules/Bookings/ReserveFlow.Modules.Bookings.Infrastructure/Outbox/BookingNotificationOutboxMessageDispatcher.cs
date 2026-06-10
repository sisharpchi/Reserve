using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Infrastructure.Outbox;
using ReserveFlow.Modules.Bookings.Domain.Bookings;
using ReserveFlow.Modules.Notifications.Application.Notifications;
using ReserveFlow.Modules.Notifications.Application.Notifications.QueueNotification;
using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Bookings.Infrastructure.Outbox;

internal sealed class BookingNotificationOutboxMessageDispatcher(
    ICommandHandler<QueueNotificationCommand, NotificationResponse> queueNotificationHandler,
    ILogger<BookingNotificationOutboxMessageDispatcher> logger) : IOutboxMessageDispatcher
{
    public async Task DispatchAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        QueueNotificationCommand[] commands = CreateNotificationCommands(message);

        if (commands.Length == 0)
        {
            LogOutboxMessageIgnored(logger, message.Id, message.Type, message.TenantId, null);
            return;
        }

        foreach (QueueNotificationCommand command in commands)
        {
            await queueNotificationHandler.Handle(command, cancellationToken);
            LogNotificationQueued(logger, message.Id, message.Type, command.TenantId, null);
        }
    }

    private static QueueNotificationCommand[] CreateNotificationCommands(OutboxMessage message)
    {
        if (IsMessageType<BookingCreatedDomainEvent>(message))
        {
            BookingCreatedDomainEvent? domainEvent = Deserialize<BookingCreatedDomainEvent>(message);

            return domainEvent is null
                ? []
                :
                [
                    CreateCommand(
                        domainEvent.TenantId,
                        "Booking created",
                        $"Booking {domainEvent.BookingId} was created for {domainEvent.StartsAtUtc:O}."),
                    CreateCommand(
                        domainEvent.TenantId,
                        "Booking reminder",
                        $"Booking {domainEvent.BookingId} starts at {domainEvent.StartsAtUtc:O}.",
                        domainEvent.StartsAtUtc.AddHours(-24).UtcDateTime)
                ];
        }

        if (IsMessageType<BookingCancelledDomainEvent>(message))
        {
            BookingCancelledDomainEvent? domainEvent = Deserialize<BookingCancelledDomainEvent>(message);

            return domainEvent is null
                ? []
                :
                [
                    CreateCommand(
                    domainEvent.TenantId,
                    "Booking cancelled",
                    $"Booking {domainEvent.BookingId} was cancelled at {domainEvent.CancelledAtUtc:O}.")
                ];
        }

        if (IsMessageType<BookingRescheduledDomainEvent>(message))
        {
            BookingRescheduledDomainEvent? domainEvent = Deserialize<BookingRescheduledDomainEvent>(message);

            return domainEvent is null
                ? []
                :
                [
                    CreateCommand(
                    domainEvent.TenantId,
                    "Booking rescheduled",
                    $"Booking {domainEvent.BookingId} was rescheduled for {domainEvent.StartsAtUtc:O}.")
                ];
        }

        if (IsMessageType<BookingMarkedAsNoShowDomainEvent>(message))
        {
            BookingMarkedAsNoShowDomainEvent? domainEvent = Deserialize<BookingMarkedAsNoShowDomainEvent>(message);

            return domainEvent is null
                ? []
                :
                [
                    CreateCommand(
                    domainEvent.TenantId,
                    "Booking marked as no-show",
                    $"Booking {domainEvent.BookingId} was marked as no-show at {domainEvent.MarkedAtUtc:O}.")
                ];
        }

        return [];
    }

    private static QueueNotificationCommand CreateCommand(
        Guid tenantId,
        string subject,
        string body,
        DateTime? deliverAtUtc = null)
    {
        return new QueueNotificationCommand(
            tenantId,
            NotificationChannel.InApp,
            $"tenant:{tenantId}",
            subject,
            body,
            deliverAtUtc);
    }

    private static bool IsMessageType<TDomainEvent>(OutboxMessage message)
    {
        return message.Type.EndsWith(typeof(TDomainEvent).Name, StringComparison.Ordinal);
    }

    private static TDomainEvent? Deserialize<TDomainEvent>(OutboxMessage message)
    {
        return JsonSerializer.Deserialize<TDomainEvent>(message.Payload);
    }

    private static readonly Action<ILogger, Guid, string, Guid?, Exception?> LogOutboxMessageIgnored =
        LoggerMessage.Define<Guid, string, Guid?>(
            LogLevel.Debug,
            new EventId(1, nameof(LogOutboxMessageIgnored)),
            "Outbox message {OutboxMessageId} with type {OutboxMessageType} and tenant {TenantId} has no notification reaction.");

    private static readonly Action<ILogger, Guid, string, Guid, Exception?> LogNotificationQueued =
        LoggerMessage.Define<Guid, string, Guid>(
            LogLevel.Information,
            new EventId(2, nameof(LogNotificationQueued)),
            "Outbox message {OutboxMessageId} with type {OutboxMessageType} queued a tenant notification for {TenantId}.");
}
