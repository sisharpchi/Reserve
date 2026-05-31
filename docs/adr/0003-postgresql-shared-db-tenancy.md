# ADR 0003: Use PostgreSQL Shared Database Tenancy

## Status

Accepted

## Context

ReserveFlow is multi-tenant. The system needs tenant isolation, but MVP should keep local development, migrations, reporting, and tests manageable.

Possible choices:

- single schema with `tenant_id`
- one database with module schemas and `tenant_id`
- tenant-per-schema
- database-per-tenant

## Decision

Use:

```text
one PostgreSQL database
+ per-module schemas
+ tenant_id columns for tenant-owned rows
+ EF Core global query filters
+ authorization and tenant membership checks
```

## Consequences

Positive:

- simple deployment
- one connection string
- clear module ownership through schemas
- good fit for EF Core 8.x
- strong enough isolation for MVP
- easier cross-tenant platform reporting

Tradeoffs:

- not as isolated as database-per-tenant
- query filters must be tested carefully
- platform admin bypasses must be explicit and audited

## Notes

Tenant-per-schema is not the MVP default. It increases migration and operational complexity and is not the primary supported EF Core multi-tenancy pattern.

Database-per-tenant can be revisited later for enterprise customers.

## References

- [EF Core multi-tenancy](https://learn.microsoft.com/en-us/ef/core/miscellaneous/multitenancy)
- [EF Core global query filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)

