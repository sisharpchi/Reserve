# Multitenancy Architecture

## Default Decision

ReserveFlow MVP uses:

```text
One PostgreSQL database
+ per-module schemas
+ tenant_id discriminator columns
+ EF Core global query filters
+ authorization checks
```

This gives strong practical isolation while keeping migrations, local development, testing, and reporting manageable.

The schema boundary is module-first, not tenant-first. For example, `bookings.bookings` contains all tenant bookings with `tenant_id`, while `bookings.outbox_messages` contains booking-module messages.

## Tenant Resolution

Tenant can be resolved from:

- public route slug: `/t/{tenantSlug}`
- admin route and authenticated user's tenant membership
- API header for internal tools: `X-Tenant-Id`
- Keycloak JWT claims plus ReserveFlow tenant membership lookup

MVP priority:

1. Public tenant slug for public booking pages.
2. Authenticated Keycloak subject mapped to ReserveFlow tenant membership for tenant admin and staff.
3. Header only for controlled internal/test scenarios.

## Tenant Context

Backend abstraction:

```csharp
public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantSlug { get; }
    string? TimeZoneId { get; }
    bool IsPlatformScope { get; }
}
```

Tenant context must be available before EF queries tenant-owned tables.

## EF Core Query Filters

Tenant-owned entities implement an interface:

```csharp
public interface ITenantEntity
{
    Guid TenantId { get; }
}
```

EF Core applies global query filters for tenant-owned rows. This reduces the chance that application code forgets `tenant_id` in every query.

Global filters are defense-in-depth, not the only protection. Authorization and handler-level tenant checks still matter.

## Comparison Of Options

### Single Public Schema Only

Shape:

```text
public.users
public.tenants
public.services
public.bookings
```

Pros:

- simplest to start
- easiest migrations
- fewer EF configuration decisions

Cons:

- weak module ownership
- tables become crowded
- harder to reason about bounded contexts
- less aligned with modular monolith learning goal

Decision: not the default. It is too flat for ReserveFlow's architecture goals.

### Module Schemas In One Database

Shape:

```text
identity.users
platform.tenants
catalog.services
bookings.bookings
bookings.outbox_messages
notifications.inbox_messages
```

Pros:

- clear module ownership
- still one database and one connection string
- manageable migrations
- works well with modular monolith
- easier to split modules later if needed

Cons:

- cross-schema joins are possible and must be controlled by code conventions
- migration organization needs discipline

Decision: default for MVP.

Implementation rule: each module owns a `DbContext`, default schema, migration history table, and optional outbox/inbox tables inside its own schema. This mirrors the Evently blueprint while preserving ReserveFlow's tenant discriminator strategy.

### Tenant-Per-Schema

Shape:

```text
tenant_smile.bookings
tenant_elite.bookings
```

Pros:

- stronger visual separation between tenants
- tenant-level backup/export can be easier in some setups

Cons:

- EF Core docs do not treat schema-per-tenant as the supported default pattern
- migrations become harder
- schema count grows with tenant count
- reporting across tenants becomes more complex
- not necessary for MVP

Decision: not used in MVP.

### Database-Per-Tenant

Shape:

```text
reserveflow_smile
reserveflow_elite
reserveflow_workhub
```

Pros:

- strongest tenant data isolation
- tenant-level restore and scaling options
- useful for enterprise customers later

Cons:

- connection management complexity
- migrations across many databases
- harder local development
- cross-tenant reporting needs aggregation
- too expensive for MVP

Decision: future enterprise option only.

## Platform Admin Bypass

Platform admin sometimes needs platform-wide queries. These queries must be explicit:

- use dedicated platform query handlers
- avoid accidental `IgnoreQueryFilters` in normal tenant handlers
- audit sensitive platform-wide access

Rule: `IgnoreQueryFilters` is not allowed in normal tenant workflows.

## Data Leak Prevention

Required controls:

- route tenant slug maps to tenant id
- user tenant membership checked for admin/staff routes
- Keycloak `sub` is mapped to a ReserveFlow local user before tenant access is granted
- EF global filters for tenant-owned entities
- command validators reject mismatched tenant ids
- database foreign keys include tenant-owned relationships where practical
- API tests cover cross-tenant access attempts
- architecture tests prevent modules from bypassing another module's data access layer

## References

- [EF Core multi-tenancy](https://learn.microsoft.com/en-us/ef/core/miscellaneous/multitenancy)
- [EF Core global query filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
