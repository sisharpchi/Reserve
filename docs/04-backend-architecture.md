# Backend Architecture

## Baseline

Backend baseline:

- .NET 8 LTS
- ASP.NET Core 8 Web API
- C# 12
- EF Core 8.x LTS
- Npgsql 8.x
- PostgreSQL
- Keycloak as the OpenID Connect/OAuth2 identity provider
- FluentValidation
- xUnit
- Testcontainers
- OpenTelemetry
- ASP.NET Core Health Checks
- Swashbuckle or NSwag for OpenAPI
- Serilog with console output and optional Seq locally
- Redis optional for cache and richer async/saga scenarios later

## Solution Shape

ReserveFlow uses the Milan Jovanovic/Evently-style project shape, renamed for the booking domain:

```text
src/
  API/
    ReserveFlow.Api/
      Program.cs
      Extensions/
      Middleware/
      OpenTelemetry/
      modules.identity.json
      modules.tenants.json
      modules.bookings.json
  Common/
    ReserveFlow.Common.Domain/
    ReserveFlow.Common.Application/
    ReserveFlow.Common.Infrastructure/
    ReserveFlow.Common.Presentation/
  Modules/
    Bookings/
      ReserveFlow.Modules.Bookings.Domain/
      ReserveFlow.Modules.Bookings.Application/
      ReserveFlow.Modules.Bookings.Infrastructure/
      ReserveFlow.Modules.Bookings.IntegrationEvents/
      ReserveFlow.Modules.Bookings.Presentation/
      ReserveFlow.Modules.Bookings.UnitTests/
      ReserveFlow.Modules.Bookings.IntegrationTests/
      ReserveFlow.Modules.Bookings.ArchitectureTests/
    Scheduling/
      ReserveFlow.Modules.Scheduling.Domain/
      ReserveFlow.Modules.Scheduling.Application/
      ReserveFlow.Modules.Scheduling.Infrastructure/
      ReserveFlow.Modules.Scheduling.IntegrationEvents/
      ReserveFlow.Modules.Scheduling.Presentation/
test/
  ReserveFlow.ArchitectureTests/
  ReserveFlow.IntegrationTests/
```

Every module follows this project pattern unless there is a clear reason to keep it smaller. The first MVP may keep API and background processing in one host, but module processors should be isolated enough that `ReserveFlow.Worker` can be split later.

Repository-wide `Directory.Build.props` should lock `net8.0`, nullable reference types, implicit usings, latest analyzer level, warnings as errors, code style enforcement, and analyzer packages.

## Building Blocks

`ReserveFlow.Common.Domain`:

- `Entity`
- `AggregateRoot`
- `ValueObject`
- `DomainEvent`
- `DomainException`
- `Result`
- `Error`

`ReserveFlow.Common.Application`:

- `ICommand`
- `ICommand<TResult>`
- `IQuery<TResult>`
- `ICommandHandler<TCommand, TResult>`
- `IQueryHandler<TQuery, TResult>`
- `IDomainEventHandler<TEvent>`
- `IIntegrationEvent`
- `IIntegrationEventHandler<TEvent>`
- `IEventBus`
- `ICurrentUser`
- `ITenantContext`
- `IClock`
- `IDbConnectionFactory`
- validation pipeline
- request logging pipeline
- exception handling pipeline
- authorization helpers

`ReserveFlow.Common.Infrastructure`:

- Npgsql data source
- Dapper connection factory for read queries and outbox processors
- JWT bearer authentication
- policy authorization and permission claims
- outbox/inbox infrastructure types
- outbox save interceptor
- time provider
- cache abstraction
- correlation id helpers
- telemetry helpers
- common event bus adapter

`ReserveFlow.Common.Presentation`:

- `IEndpoint`
- endpoint registration extensions
- Result-to-ProblemDetails mapping
- common API result helpers

## API Composition Root

`ReserveFlow.Api` owns startup wiring only. It should:

1. Configure Serilog, ProblemDetails, global exception handling, OpenAPI, and health checks.
2. Load module application assemblies through `AssemblyReference`.
3. Register application behaviors and validators from module assemblies.
4. Register common infrastructure: JWT bearer auth, policy authorization, Npgsql, OpenTelemetry, cache, event bus, and outbox interceptor.
5. Load module config files such as `modules.bookings.json` and `modules.bookings.Development.json`.
6. Call each module registration method, for example `AddBookingsModule(configuration)`.
7. Map health checks, auth/authorization middleware, and registered module endpoints.

The API project references module `Infrastructure` projects only for composition. Application code inside modules must not depend on the API host.

## Module Registration

Each module exposes one infrastructure registration extension:

```text
AddBookingsModule(configuration)
AddSchedulingModule(configuration)
AddNotificationsModule(configuration)
```

That extension registers:

- module domain event handlers
- module integration event handlers
- module EF Core `DbContext`
- module repositories and unit of work
- module outbox/inbox options and processors
- module presentation endpoints from `Presentation.AssemblyReference.Assembly`

Module config belongs in API-level files:

```text
modules.bookings.json
modules.bookings.Development.json
modules.notifications.json
modules.notifications.Development.json
```

This keeps module settings discoverable without turning `appsettings.json` into one giant file.

## CQRS And Vertical Slices

Each use case lives in its own slice.

Example:

```text
Modules/Bookings/Application/CreateBooking/
  CreateBookingCommand.cs
  CreateBookingCommandHandler.cs
  CreateBookingValidator.cs
  CreateBookingResponse.cs
```

Commands change state. Queries read state. Handlers are small orchestration units that load data, apply rules, call domain methods, save changes, and return DTOs.

The command/query contracts should be MediatR-compatible even if the MVP starts with a custom lightweight dispatcher. If MediatR is used later, it should sit behind the same `ICommand/IQuery` shape with validation, logging, and exception pipelines.

## Module Responsibilities

Identity:

- local app user profile mapped to Keycloak subject
- roles
- permissions
- Keycloak realm/client integration
- JWT bearer token validation
- tenant memberships

Tenants:

- tenant profile
- tenant slug
- category
- status
- settings
- owner assignment

Catalog:

- services
- duration
- price
- buffers
- staff/resource requirements

Staffing:

- staff members
- staff working hours
- staff unavailable periods
- staff-service assignment

Resources:

- rooms, chairs, equipment, meeting rooms
- resource working hours
- resource unavailable periods

Scheduling:

- available slot calculation
- overlap checks
- timezone handling
- "Any available" logic

Bookings:

- booking aggregate
- customer
- create/cancel/reschedule/complete/no-show
- booking history
- idempotency
- optimistic concurrency
- module-owned outbox

Notifications:

- notification messages
- templates
- delivery attempts
- fake/email-ready sender
- module inbox for booking integration events

Reporting:

- daily bookings
- staff utilization
- cancellation/no-show reports

Audit:

- sensitive action tracking

## Domain Events

Domain events represent something that already happened:

- `BookingCreatedDomainEvent`
- `BookingCancelledDomainEvent`
- `BookingRescheduledDomainEvent`
- `BookingCompletedDomainEvent`
- `NoShowDetectedDomainEvent`
- `TenantProvisionedDomainEvent`

Domain events keep command handlers from becoming too large. A booking command should not directly know all side effects like notification, audit, reporting, and reminder scheduling.

## Outbox And Inbox Flow

Booking creation should save both booking data and a module-owned outbox message in the same database transaction.

```text
Create booking command
  -> validate tenant, service, staff/resource, availability
  -> create booking aggregate
  -> add domain event
  -> EF Core save interceptor inserts bookings.outbox_messages
  -> module outbox processor dispatches domain event handlers
  -> handler writes notification command/integration event
  -> notifications module sends fake/email-ready notification
```

Outbox is implemented first for notification and reminder reliability. Inbox is part of the module template, but only active for modules consuming integration events or external callbacks.

## Background Work

MVP can use ASP.NET Core `BackgroundService` for:

- outbox processor
- stale pending booking cleanup
- reminder enqueue/check
- failed notification retry

The Evently blueprint uses Quartz for recurring module jobs. ReserveFlow can start with `BackgroundService`; move to Quartz or Hangfire when dashboarded delayed jobs, recurring jobs, and manual retry controls become valuable.

## Endpoint Style

ReserveFlow uses module-owned minimal endpoint classes:

```text
ReserveFlow.Modules.Bookings.Presentation/CreateBookingEndpoint
ReserveFlow.Modules.Bookings.Presentation/CancelBookingEndpoint
ReserveFlow.Modules.Scheduling.Presentation/GetAvailableSlotsEndpoint
```

Each endpoint maps a route, applies authorization metadata, sends one command/query, and converts `Result` into an HTTP response. Endpoint classes should not contain business rules.

## Validation And Authorization

FluentValidation validates input DTOs and commands.

Authentication uses Keycloak:

- Angular signs users in with Keycloak using Authorization Code + PKCE.
- Keycloak issues access tokens.
- ASP.NET Core 8 validates bearer tokens with `Microsoft.AspNetCore.Authentication.JwtBearer`.
- `sub`, `email`, `preferred_username`, realm roles, and client roles are mapped into `ICurrentUser`.
- Passwords, registration, password reset, MFA, and SSO are owned by Keycloak, not ReserveFlow.

Authorization uses:

- ASP.NET Core policies
- role claims
- permission claims
- tenant membership checks
- resource-based checks for bookings and schedules

Examples:

- Customer can cancel own booking only before policy deadline.
- Staff can manage assigned bookings.
- Tenant Admin can manage bookings in own tenant.
- Platform Admin can manage tenants, not tenant-local scheduling by default.

## References

- [ASP.NET Core authorization policies](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)
- [ASP.NET Core JWT bearer authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication)
- [ASP.NET Core hosted services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [ASP.NET Core Swagger/OpenAPI for ASP.NET Core 8](https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-8.0)
- [Keycloak securing applications overview](https://www.keycloak.org/securing-apps/overview)
- [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd)
- [jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture)
- [ardalis/CleanArchitecture](https://github.com/ardalis/CleanArchitecture)
