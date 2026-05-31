# ADR 0004: Use Outbox From The Beginning

## Status

Accepted

## Context

Booking changes trigger side effects:

- confirmation notification
- cancellation notification
- reminder scheduling
- audit history
- reporting updates
- future webhooks

If the booking is saved but the notification fails, users may not be informed. If the notification is sent before the transaction commits, users may receive messages for bookings that do not exist.

## Decision

Use outbox from the first booking MVP:

1. Save booking state and outbox message in the same PostgreSQL transaction.
2. Store outbox messages in the owning module schema, for example `bookings.outbox_messages`.
3. Process unprocessed outbox messages with an ASP.NET Core `BackgroundService` first.
4. Keep the processor replaceable with Quartz or Hangfire later.
5. Mark processed messages with timestamp.
6. Store error and retry count on failures.
7. Track message consumers so handlers can be idempotent.
8. Use fake notification sender first, then real providers later.

## Consequences

Positive:

- reliable notification workflow
- restart-safe background processing
- clear audit trail of integration messages
- easier future webhook/event integration

Tradeoffs:

- extra table and processor from early MVP
- each event-producing module owns its own outbox table
- messages are eventually processed, not sent inside request
- idempotent handlers are needed as the system grows

## Future

Use module inbox tables for external webhooks and for integration events consumed from other modules. Examples: `notifications.inbox_messages`, `integrations.inbox_messages`, and later `billing.inbox_messages`.

## References

- [ASP.NET Core hosted services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd)
