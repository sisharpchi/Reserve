# Backend Implementation Plan

## M0 Foundation

Goal: create a runnable backend foundation.

Tasks:

- Create solution and project structure.
- Add `Common.Domain`, `Common.Application`, `Common.Infrastructure`, and `Common.Presentation` projects.
- Add module project template: `Domain`, `Application`, `Infrastructure`, `IntegrationEvents`, `Presentation`, `UnitTests`, `IntegrationTests`, and `ArchitectureTests`.
- Add `Directory.Build.props` with `net8.0`, nullable reference types, implicit usings, latest analyzer level, warnings as errors, and code style enforcement.
- Create ASP.NET Core 8 API host.
- Configure PostgreSQL and EF Core 8.x.
- Add Docker Compose for API, PostgreSQL, Keycloak, and optional Seq/Redis/Jaeger profiles.
- Add global exception handling with ProblemDetails.
- Add Serilog structured logging and correlation id.
- Add health endpoints: `/health/live`, `/health/ready`.
- Add OpenAPI using Swashbuckle or NSwag.
- Add automatic module endpoint registration through `Common.Presentation`.
- Add API extension to load `modules.{module}.json` and `modules.{module}.Development.json`.
- Add CI for restore, build, test.

Acceptance:

- API starts locally.
- PostgreSQL and Keycloak are available through Docker Compose.
- Module config files are loaded.
- Endpoint discovery maps at least one endpoint from a module `Presentation` project.
- First migration applies to clean database.
- Health endpoints return expected status.
- OpenAPI document is available in development.

## M1 Identity And Tenancy

Goal: users can authenticate and access tenant-scoped areas safely.

Tasks:

- Create local app user, role, permission, and tenant user models mapped to Keycloak `sub`.
- Configure Keycloak realm, clients, roles, redirect URIs, and local seed users.
- Add a checked-in development realm export for local Docker Compose.
- Configure ASP.NET Core JWT bearer validation against Keycloak authority/audience.
- Add Keycloak health check to readiness checks.
- Implement current user endpoint from Keycloak claims plus ReserveFlow permissions.
- Implement admin-side user invitation/provisioning flow through backend-only Keycloak Admin REST API or document manual MVP setup.
- Seed initial platform admin.
- Implement tenant context from route/header/claim.
- Add permission policies.
- Add tenant isolation guard.

Acceptance:

- Platform admin can log in.
- API accepts valid Keycloak access token and rejects invalid tokens.
- Tenant user cannot access another tenant.
- Protected endpoint requires correct permission.

## M2 Platform Admin

Goal: platform owner can manage tenants and categories.

Tasks:

- Create global category model.
- Create tenant model with slug, status, timezone.
- Add tenant CRUD.
- Add activate/suspend commands.
- Add assign tenant owner command.
- Add platform usage query.
- Add platform audit records.

Acceptance:

- Platform admin can create a tenant and assign owner.
- Suspended tenant cannot accept new public bookings.
- Tenant slug is unique.

## M3 Tenant Admin Core

Goal: tenant admin can configure the business.

Tasks:

- Create services with duration, price, buffers, active flag.
- Create staff members and service assignment.
- Create resources and resource requirements.
- Add weekly working hours.
- Add unavailable periods.
- Add tenant booking policy.
- Protect all tenant admin endpoints.

Acceptance:

- Tenant admin can configure a demo clinic or coworking tenant.
- Staff and resources can be assigned to services.
- Cross-tenant admin access is blocked.

## M4 Scheduling Engine

Goal: API can calculate correct available slots.

Tasks:

- Define availability request/response DTOs.
- Load tenant timezone and convert requested date to UTC range.
- Calculate service duration and buffers.
- Resolve staff/resource candidates.
- Subtract unavailable periods and existing bookings.
- Implement "Any available" grouping.
- Add overlap checks.
- Add query indexes.

Acceptance:

- Working hours, lunch breaks, unavailable periods, and bookings affect slot output.
- Existing booking blocks overlapping slots.
- Tenant-local times are displayed correctly.

## M5 Booking Core

Goal: customer and admin can manage the booking lifecycle.

Tasks:

- Create customer entity.
- Create booking aggregate and booking statuses.
- Add create booking command.
- Add cancel booking command with policy checks.
- Add reschedule booking command.
- Add complete and no-show commands.
- Add booking history.
- Add idempotency key for public create booking.
- Add optimistic concurrency handling.

Acceptance:

- Booking can be created only for an available slot.
- Cancelled/completed booking cannot be rescheduled.
- Duplicate create retries do not create duplicate bookings.
- Race condition double booking is blocked.

## M6 Notifications And Outbox

Goal: booking side effects are reliable.

Tasks:

- Create module-owned outbox and inbox tables in the module schema.
- Save outbox messages from booking events using a common EF Core save interceptor.
- Implement module outbox processor as `BackgroundService` first.
- Add idempotent domain event handler tracking.
- Add inbox processor template for modules that consume integration events.
- Create `INotificationSender`.
- Implement fake notification sender.
- Queue booking confirmation and cancellation notifications.
- Queue reminder messages.
- Add retry count and error tracking.

Acceptance:

- Booking creation creates an outbox record.
- Outbox record is stored in the booking module schema.
- Outbox processor marks successful message as processed.
- Failed messages keep error and retry count.
- Duplicate event handling is idempotent.

## M7 Public API Support

Goal: frontend can build public booking flow.

Tasks:

- Public categories endpoint.
- Public tenants endpoint with search/category filter.
- Public tenant profile endpoint.
- Public active services endpoint.
- Public availability endpoint.
- Public booking lookup by code/token.

Acceptance:

- Customer UI can browse from category to booking confirmation using API only.

## M8 Admin API Polish

Goal: admin workflows are consistent and usable.

Tasks:

- Add pagination, filtering, sorting to lists.
- Add consistent validation errors.
- Add audit log queries.
- Add notification/outbox log queries.
- Add report endpoints.

Acceptance:

- Admin screens can show production-like tables and details.

## M9 Hardening

Goal: production readiness.

Tasks:

- Integration tests with PostgreSQL Testcontainers.
- API tests with `WebApplicationFactory`.
- Module-level architecture tests for layer rules.
- Solution-level architecture tests for cross-module dependency rules.
- Rate limiting for auth and public booking endpoints.
- Security headers and CORS.
- OpenTelemetry traces and metrics.
- Query performance review and indexes.
- Dependency audit in CI.

Acceptance:

- Core flows are tested.
- Tenant isolation and booking overlap are tested.
- CI passes.

## M10 Release

Goal: releaseable demo MVP.

Tasks:

- Production Dockerfile.
- Worker deployment mode documented.
- Migration runner strategy.
- Seed demo tenant, services, staff, resources.
- Production config docs.
- README and screenshots.
- Release checklist.

Acceptance:

- New developer can run local demo from docs.
- Production container image builds.
- Demo booking flow works end to end.
