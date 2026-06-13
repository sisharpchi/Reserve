# Database Architecture

## Baseline

ReserveFlow uses PostgreSQL as the primary database.

Database baseline:

- One physical PostgreSQL database.
- Per-module schemas.
- Tenant-owned tables include `tenant_id`.
- EF Core 8.x migrations.
- Npgsql 8.x provider.
- JSONB for module outbox/inbox payloads, audit values, integration metadata, and extension settings.

## Schemas

Recommended schemas:

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
```

Schema ownership:

- `identity`: users, roles, permissions, tenant membership.
- `platform`: tenants, categories, platform settings.
- `catalog`: services and service requirements.
- `staffing`: staff members and staff schedules.
- `resources`: resources and resource schedules.
- `scheduling`: scheduling read models or shared schedule support tables.
- `bookings`: customers, bookings, booking history.
- `notifications`: notification messages and delivery attempts.
- `reporting`: reporting projections.
- `audit`: audit logs.

Each module that publishes or consumes events also owns its own:

- `<module>.outbox_messages`
- `<module>.outbox_message_consumers`
- `<module>.inbox_messages`
- `<module>.inbox_message_consumers`

This follows the Milan Jovanovic/Evently blueprint: common outbox/inbox infrastructure types are reused, but the physical tables live in the owning module schema.

## Core Tables

Platform:

- `platform.categories`
- `platform.tenants`
- `platform.tenant_settings`

Identity:

- `identity.users`
- `identity.roles`
- `identity.permissions`
- `identity.user_roles`
- `identity.tenant_users`
- `identity.external_identity_links`

Catalog:

- `catalog.services`
- `catalog.service_staff`
- `catalog.service_resources`

Staffing:

- `staffing.staff_members`
- `staffing.staff_working_hours`
- `staffing.staff_unavailable_periods`

Resources:

- `resources.resources`
- `resources.resource_types`
- `resources.resource_working_hours`
- `resources.resource_unavailable_periods`

Bookings:

- `bookings.customers`
- `bookings.bookings`
- `bookings.booking_history`
- `bookings.booking_idempotency_keys`

Notifications:

- `notifications.notification_messages`
- `notifications.notification_attempts`

Module messaging:

- `bookings.outbox_messages`
- `bookings.outbox_message_consumers`
- `notifications.inbox_messages`
- `notifications.inbox_message_consumers`
- other module-owned outbox/inbox tables when needed

Audit:

- `audit.audit_logs`

## Tenant-Owned Columns

Tenant-owned tables include:

- `tenant_id uuid not null`
- `created_at_utc timestamptz not null`
- `updated_at_utc timestamptz null`

Examples:

- `catalog.services.tenant_id`
- `staffing.staff_members.tenant_id`
- `resources.resources.tenant_id`
- `bookings.bookings.tenant_id`
- `bookings.customers.tenant_id`

Global tables do not need `tenant_id`:

- `platform.categories`
- `platform.tenants`
- some `identity.roles` and `identity.permissions`

`identity.users` stores the local application profile and the Keycloak subject (`keycloak_subject`). ReserveFlow does not store password hashes or refresh tokens because Keycloak owns authentication credentials and token issuance.

## Booking Table

Important columns:

```sql
id uuid primary key,
tenant_id uuid not null,
customer_id uuid not null,
service_id uuid not null,
staff_member_id uuid null,
resource_id uuid null,
starts_at_utc timestamptz not null,
ends_at_utc timestamptz not null,
status varchar(40) not null,
price_snapshot numeric(18,2) null,
currency varchar(10) null,
booking_code varchar(80) not null,
row_version xmin,
created_at_utc timestamptz not null,
cancelled_at_utc timestamptz null
```

`staff_member_id` is nullable because coworking bookings may be resource-only.

`resource_id` is nullable because coaching or consulting bookings may be staff-only.

## Overlap Formula

The core interval overlap rule:

```text
existing.starts_at_utc < requested_end_utc
AND requested_start_utc < existing.ends_at_utc
```

This must be applied for staff and resources.

## Indexes

Booking indexes:

```sql
create index ix_bookings_tenant_staff_time
on bookings.bookings (tenant_id, staff_member_id, starts_at_utc, ends_at_utc);

create index ix_bookings_tenant_resource_time
on bookings.bookings (tenant_id, resource_id, starts_at_utc, ends_at_utc);

create index ix_bookings_tenant_status_time
on bookings.bookings (tenant_id, status, starts_at_utc);
```

Service indexes:

```sql
create index ix_services_tenant_active
on catalog.services (tenant_id, is_active);
```

Staff/resource indexes:

```sql
create index ix_staff_tenant_active
on staffing.staff_members (tenant_id, is_active);

create index ix_resources_tenant_active
on resources.resources (tenant_id, is_active);
```

## Constraints

Use database constraints for invariants that must never be broken:

- positive service duration
- `ends_at_utc > starts_at_utc`
- unique tenant slug
- unique booking code
- unique category slug
- unique active idempotency key per tenant/customer/request

For double-booking safety, evaluate PostgreSQL exclusion constraints for active bookings after the first MVP migration is stable. Application-level checks are not enough under concurrency.

## JSONB Usage

Recommended JSONB columns:

- `<module>.outbox_messages.payload`
- `<module>.inbox_messages.payload`
- `audit.audit_logs.old_values`
- `audit.audit_logs.new_values`
- `platform.tenant_settings.metadata`
- `notifications.notification_messages.metadata`

JSONB is for flexible metadata and message payloads, not for core relational booking data.

## Migrations

Migration rules:

- All schema changes go through EF Core migrations.
- Each module owns its own `DbContext`.
- Each module stores EF migration history in its own schema.
- Migrations must be tested on a clean database.
- Production release should use an explicit migration runner, not automatic migrations inside normal API startup.
- Seed data should be deterministic and environment-aware.

## Local Database Startup

For local Docker Compose startup, the API waits for the PostgreSQL container health check before it starts. The development schema initializer also retries transient PostgreSQL startup failures such as `57P03: the database system is starting up`, short socket refusals, and timeouts.

This retry is a local/development resilience feature. It should not replace a production migration runner or deployment-level dependency checks.

## Migration Runner Strategy

Production deployments should apply EF Core migrations with `ReserveFlow.MigrationService` before starting or rolling the API containers. The migrator is a one-shot .NET 8 console process that registers every module infrastructure project and applies migrations in module order: identity, platform, catalog, staffing, resources, scheduling, bookings, notifications, audit, reporting, and integrations.

Local Docker Compose exposes this as a profile:

```powershell
docker compose --profile migrations run --rm reserveflow.migrations
```

The API may still create schemas and seed demo data in development, but production release flow should keep schema changes explicit, observable, and separate from normal request-serving startup.

## Module DbContext Rule

Each module has a database context with its default schema:

```text
IdentityDbContext -> identity
CatalogDbContext -> catalog
SchedulingDbContext -> scheduling
BookingsDbContext -> bookings
NotificationsDbContext -> notifications
```

The module context applies the common outbox/inbox entity configurations only when that module needs event processing. Cross-module data access through another module's `DbContext` is not allowed.

## References

- [EF Core 8 release notes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew)
- [EF Core multi-tenancy](https://learn.microsoft.com/en-us/ef/core/miscellaneous/multitenancy)
- [EF Core global query filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
