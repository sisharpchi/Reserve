# Reference Architecture Blueprint

## Purpose

This document explains how ReserveFlow combines the studied references into one consistent architecture. The strongest implementation blueprint is the local Milan Jovanovic/Evently modular monolith code, adapted to ReserveFlow's appointment and resource-booking domain.

ReserveFlow must not copy another project blindly. The goal is to reuse the architectural shape: module boundaries, composition root, endpoint registration, outbox/inbox reliability, architecture tests, and Keycloak integration.

## Source Synthesis

Milan Jovanovic/Evently blueprint:

- Use `Common.Domain`, `Common.Application`, `Common.Infrastructure`, and `Common.Presentation` projects.
- Use per-module projects: `Domain`, `Application`, `Infrastructure`, `IntegrationEvents`, and `Presentation`.
- API host references module `Infrastructure` projects for composition.
- Module application assemblies expose an `AssemblyReference` for command/query/validator registration.
- Module presentation assemblies expose endpoints registered by convention.
- API loads module-specific config files such as `modules.users.json`.
- Each module has its own EF Core `DbContext`, schema, migration history, outbox, and inbox processing.
- Keycloak runs in local Docker Compose and is included in API health checks.
- Serilog, Seq, Redis, Jaeger/OpenTelemetry, and strict analyzer settings are part of the development baseline.
- Architecture tests enforce layer and module dependency rules.

kgrzybek/modular-monolith-with-ddd:

- Treat modules as bounded contexts, not folders of technical layers only.
- Keep domain model expressive and independent from frameworks.
- Prefer domain events for business facts and integration events for cross-module/external communication.
- Use outbox for reliable messaging.

Jason Taylor CleanArchitecture:

- Keep web/API thin.
- Put application use cases in clear request/handler slices.
- Use validation, authorization, and testing as first-class concerns.
- Keep infrastructure details behind application abstractions.

Ardalis CleanArchitecture:

- Keep the domain model central.
- Prefer use-case-oriented application code over generic service layers.
- Keep endpoints/controllers small and obvious.
- Use tests to protect behavior, not just implementation details.

Keycloak securing applications overview:

- Keycloak is the OpenID Connect/OAuth2 identity provider.
- Angular uses Authorization Code + PKCE.
- ASP.NET Core validates bearer tokens.
- ReserveFlow owns tenant membership, permissions, and resource authorization.

## ReserveFlow Backend Shape

Target backend shape:

```text
ReserveFlow.Api
ReserveFlow.Common.Domain
ReserveFlow.Common.Application
ReserveFlow.Common.Infrastructure
ReserveFlow.Common.Presentation

ReserveFlow.Modules.Identity.Domain
ReserveFlow.Modules.Identity.Application
ReserveFlow.Modules.Identity.Infrastructure
ReserveFlow.Modules.Identity.IntegrationEvents
ReserveFlow.Modules.Identity.Presentation

ReserveFlow.Modules.Bookings.Domain
ReserveFlow.Modules.Bookings.Application
ReserveFlow.Modules.Bookings.Infrastructure
ReserveFlow.Modules.Bookings.IntegrationEvents
ReserveFlow.Modules.Bookings.Presentation
```

The same pattern applies to Tenants, Catalog, Staffing, Resources, Scheduling, Notifications, Reporting, Audit, and Integrations.

## API Startup Pattern

The API startup should follow this order:

1. Add Serilog, ProblemDetails, global exception handler, OpenAPI, and health checks.
2. Create an array of module application assemblies.
3. Register application behaviors and validators from those assemblies.
4. Register common infrastructure.
5. Load module config files.
6. Register every module through `Add{Module}Module(configuration)`.
7. Map health checks, auth middleware, and module endpoints.

This keeps the API host boring in the best way: it composes modules but does not contain domain workflow logic.

## Module Internal Pattern

Each module registration should do the same things:

- Register domain event handlers from the module application assembly.
- Register integration event handlers from the module presentation/integration assembly.
- Register the module `DbContext`.
- Register repositories and `IUnitOfWork`.
- Register module options from `modules.{module}.json`.
- Register outbox and inbox processors when needed.
- Register endpoints from the module presentation assembly.

ReserveFlow modules should look predictable. If a developer learns Bookings, they should be able to navigate Scheduling or Notifications quickly.

## Endpoint Pattern

Endpoints live in module `Presentation` projects. An endpoint should:

- map exactly one command/query flow when possible
- apply route, auth policy, tags, and request binding
- call the dispatcher/sender
- return `Result` mapped to HTTP response or ProblemDetails

Endpoints must not calculate availability, enforce booking lifecycle rules, or send notifications directly.

## Database Pattern

Use one PostgreSQL database with module schemas:

```text
identity
platform
catalog
staffing
resources
scheduling
bookings
notifications
reporting
audit
integrations
```

Tenant-owned rows include `tenant_id`. Module-owned outbox/inbox tables live in the same schema as the module:

```text
bookings.outbox_messages
bookings.outbox_message_consumers
notifications.inbox_messages
notifications.inbox_message_consumers
```

Each module has its own EF Core `DbContext` and migration history table in its schema.

## Testing Pattern

Testing follows the Evently-style split:

- module unit tests for domain rules
- module integration tests for EF Core and application handlers
- module architecture tests for layer boundaries
- solution architecture tests for cross-module dependencies
- API/system integration tests for public booking and admin flows

Critical ReserveFlow-specific test areas:

- tenant isolation
- booking overlap prevention
- timezone conversion
- Keycloak token validation
- outbox/inbox idempotency
- permission and resource authorization

## Local Development Pattern

Docker Compose should include:

- `reserveflow.api`
- `reserveflow.database`
- `reserveflow.identity` using Keycloak and a development realm import
- optional `reserveflow.seq`
- optional `reserveflow.redis`
- optional `reserveflow.jaeger`

MVP can run API and worker logic in one process. The module processor design should still allow a future `ReserveFlow.Worker`.

## References

- [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd)
- [jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture)
- [ardalis/CleanArchitecture](https://github.com/ardalis/CleanArchitecture)
- [Keycloak securing applications overview](https://www.keycloak.org/securing-apps/overview)
