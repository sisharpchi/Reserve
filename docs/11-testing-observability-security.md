# Testing, Observability, And Security

## Testing Strategy

ReserveFlow needs tests at multiple levels because the hardest bugs are not syntax bugs. They are tenant isolation, time calculations, and booking conflicts.

## Unit Tests

Focus:

- domain rules
- value objects
- booking lifecycle
- availability calculation helpers
- cancellation policy

Examples:

- Cannot cancel completed booking.
- Cannot reschedule cancelled booking.
- Cannot create booking in the past.
- Cannot create booking outside working hours.
- Cannot create overlapping booking.
- Cancellation deadline blocks customer cancellation.
- Tenant admin override follows policy.

Unit tests should not need a database.

## Integration Tests

Use PostgreSQL with Testcontainers.

Focus:

- EF Core mappings
- migrations
- global query filters
- indexes and constraints
- transaction behavior
- outbox persistence
- booking overlap under real SQL behavior

Do not use EF Core in-memory provider for database behavior. It does not represent PostgreSQL semantics.

## API Tests

Use `WebApplicationFactory`.

Focus:

- Keycloak token validation flow
- public booking flow
- tenant admin CRUD
- platform admin tenant management
- staff schedule actions
- authorization failures
- cross-tenant access attempts

Critical scenarios:

- Tenant Admin A cannot read Tenant B bookings.
- Customer booking create returns booking code.
- Suspended tenant rejects public booking creation.
- Staff cannot complete booking assigned to another staff member.

## Architecture Tests

Use architecture tests to protect boundaries:

- Domain must not reference Infrastructure.
- Domain must not reference ASP.NET Core.
- Application must not reference API.
- Application must not reference Presentation.
- Presentation must not reference Infrastructure.
- Module internals are not exposed unnecessarily.
- One module must not depend on another module except through explicit integration events or approved contracts.
- Commands, queries, handlers, validators, and domain events follow naming and visibility conventions.
- Normal tenant handlers must not use unrestricted query filter bypass.

## Performance Smoke Tests

Use NBomber later for baseline scenarios:

- 100 users search availability.
- 50 users create bookings.
- Peak hour booking attempts with limited staff/resources.

Performance testing should start after the scheduling engine and booking core are stable.

## Observability

Logging:

- structured logs
- correlation id
- tenant id
- user id where available
- booking id for booking flows
- no sensitive customer data in logs

Tracing:

- HTTP request span
- command/query handler span
- availability calculation span
- EF Core query visibility
- module outbox/inbox processing span
- notification sending span
- optional MassTransit/Quartz spans if those packages are adopted later

Metrics:

- `booking_created_total`
- `booking_cancelled_total`
- `availability_search_duration_ms`
- `outbox_pending_messages`
- `notification_failed_total`
- `active_tenants_total`
- `booking_conflict_total`

Health checks:

- `/health/live`: process is running
- `/health/ready`: database, Keycloak, outbox processor readiness, critical dependencies

Local observability profile:

- Serilog console logs by default.
- Optional Seq container for local structured log browsing.
- Optional Jaeger or OpenTelemetry collector profile for trace inspection.
- Optional Redis health check when Redis-backed cache or saga storage is enabled.

Local development should support OpenTelemetry-compatible tooling. Aspire dashboard can be considered for local observability if it fits the .NET 8 project setup.

## Security

Authentication:

- Keycloak owns login, registration, password reset, MFA, SSO, access tokens, and refresh tokens.
- ASP.NET Core validates Keycloak-issued JWT bearer tokens.
- ReserveFlow maps Keycloak `sub` to a local app user.
- ReserveFlow stores tenant memberships, permissions, and resource authorization data.

Authorization:

- policy-based permissions
- tenant membership checks
- resource-based checks for booking ownership and staff assignment

Tenant isolation:

- tenant context required for tenant data
- EF query filters
- cross-tenant API tests
- no tenant id from body trusted without server-side verification

Public endpoint protection:

- rate limit login
- rate limit booking create
- rate limit availability search
- validation on all inputs

Login rate limiting primarily belongs to Keycloak. ReserveFlow still rate limits public booking and availability endpoints and any local auth context endpoints such as `/api/auth/me`.

Audit:

- tenant created
- tenant suspended/activated
- owner assigned
- role/permission assigned
- service price changed
- working hours changed
- booking cancelled by admin
- no-show marked

Secrets:

- no secrets committed
- environment variables or secret store for production
- local development uses safe sample settings

## Definition Of Done

Backend task is done when:

- validation exists
- authorization is enforced
- tenant isolation is respected
- migration exists if database changed
- useful logs/errors exist
- tests are added where relevant
- OpenAPI output matches endpoint behavior

Frontend task is done when:

- loading, empty, and error states exist
- forms validate input
- API integration works
- role/permission restrictions are reflected in UI
- responsive layout is acceptable
- no major console errors remain

Feature is production-ready when:

- backend and frontend happy path works
- main failure paths are handled
- authorization and tenant isolation are tested
- observability is present
- documentation is updated

## References

- [ASP.NET Core hosted services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [ASP.NET Core authorization policies](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)
- [ASP.NET Core JWT bearer authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication)
- [Keycloak securing applications overview](https://www.keycloak.org/securing-apps/overview)
- [EF Core global query filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
