using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Infrastructure.Outbox;
using ReserveFlow.Modules.Bookings.Domain.Bookings;
using ReserveFlow.Modules.Notifications.Application.Notifications;
using ReserveFlow.Modules.Notifications.Application.Notifications.CancelPendingNotifications;
using ReserveFlow.Modules.Notifications.Application.Notifications.QueueNotification;
using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Bookings.Infrastructure.Outbox;

internal sealed class BookingNotificationOutboxMessageDispatcher(
    ICommandHandler<QueueNotificationCommand, NotificationResponse> queueNotificationHandler,
    ICommandHandler<CancelPendingNotificationsCommand, int> cancelPendingNotificationsHandler,
    ILogger<BookingNotificationOutboxMessageDispatcher> logger) : IOutboxMessageDispatcher
{
    public async Task DispatchAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        CancelPendingNotificationsCommand? cancelCommand = CreateCancelCommand(message);
        QueueNotificationCommand[] commands = CreateNotificationCommands(message);

        if (cancelCommand is null && commands.Length == 0)
        {
            LogOutboxMessageIgnored(logger, message.Id, message.Type, message.TenantId, null);
            return;
        }

        if (cancelCommand is not null)
        {
            int cancelledCount = await cancelPendingNotificationsHandler.Handle(cancelCommand, cancellationToken);
            LogNotificationsCancelled(logger, message.Id, message.Type, cancelCommand.TenantId, cancelledCount, null);
        }

        foreach (QueueNotificationCommand command in commands)
        {
            await queueNotificationHandler.Handle(command, cancellationToken);
            LogNotificationQueued(logger, message.Id, message.Type, command.TenantId, null);
        }
    }

    private static CancelPendingNotificationsCommand? CreateCancelCommand(OutboxMessage message)
    {
        if (IsMessageType<BookingCancelledDomainEvent>(message))
        {
            BookingCancelledDomainEvent? domainEvent = Deserialize<BookingCancelledDomainEvent>(message);

            return domainEvent is null
                ? null
                : new CancelPendingNotificationsCommand(
                    domainEvent.TenantId,
                    ReminderCorrelationKey(domainEvent.BookingId),
                    "Booking was cancelled.");
        }

        if (IsMessageType<BookingRescheduledDomainEvent>(message))
        {
            BookingRescheduledDomainEvent? domainEvent = Deserialize<BookingRescheduledDomainEvent>(message);

            return domainEvent is null
                ? null
                : new CancelPendingNotificationsCommand(
                    domainEvent.TenantId,
                    ReminderCorrelationKey(domainEvent.BookingId),
                    "Booking was rescheduled.");
        }

        return null;
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
                        domainEvent.StartsAtUtc.AddHours(-24).UtcDateTime,
                        ReminderCorrelationKey(domainEvent.BookingId))
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
                        $"Booking {domainEvent.BookingId} was rescheduled for {domainEvent.StartsAtUtc:O}."),
                    CreateCommand(
                        domainEvent.TenantId,
                        "Booking reminder",
                        $"Booking {domainEvent.BookingId} starts at {domainEvent.StartsAtUtc:O}.",
                        domainEvent.StartsAtUtc.AddHours(-24).UtcDateTime,
                        ReminderCorrelationKey(domainEvent.BookingId))
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
        DateTime? deliverAtUtc = null,
        string? correlationKey = null)
    {
        return new QueueNotificationCommand(
            tenantId,
            NotificationChannel.InApp,
            $"tenant:{tenantId}",
            subject,
            body,
            deliverAtUtc,
            correlationKey);
    }

    private static string ReminderCorrelationKey(Guid bookingId)
    {
        return $"booking:{bookingId}:reminder";
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

    private static readonly Action<ILogger, Guid, string, Guid, int, Exception?> LogNotificationsCancelled =
        LoggerMessage.Define<Guid, string, Guid, int>(
            LogLevel.Information,
            new EventId(3, nameof(LogNotificationsCancelled)),
            "Outbox message {OutboxMessageId} with type {OutboxMessageType} cancelled {CancelledCount} pending booking reminder notifications for tenant {TenantId}.");
}
