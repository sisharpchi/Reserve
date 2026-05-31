# ReserveFlow Documentation

ReserveFlow is a multi-tenant appointment and resource booking SaaS platform. It is designed for clinics, salons, consultants, coaching businesses, coworking spaces, and internal company scheduling.

The baseline for this project is:

- .NET 8 LTS, ASP.NET Core 8, C# 12
- EF Core 8.x LTS with Npgsql 8.x
- PostgreSQL with one physical database, module schemas, and tenant-owned rows using `tenant_id`
- Keycloak as the OpenID Connect/OAuth2 identity provider
- Angular SPA using the latest stable Angular version compatible with the selected Node LTS during implementation
- Modular monolith, Clean Architecture, vertical slices, lightweight CQRS, domain events, and outbox
- Milan Jovanovic/Evently-style module layout: `Common.*` building blocks, per-module `Domain/Application/Infrastructure/IntegrationEvents/Presentation` projects, module config files, automatic endpoint registration, and architecture tests

## Reading Order

1. [Product System Design](01-product-system-design.md)
2. [Users And Flows](02-users-and-flows.md)
3. [Overall Architecture](03-overall-architecture.md)
4. [Backend Architecture](04-backend-architecture.md)
5. [Backend Implementation Plan](05-backend-implementation-plan.md)
6. [Frontend Architecture](06-frontend-architecture.md)
7. [Frontend Implementation Plan](07-frontend-implementation-plan.md)
8. [Database Architecture](08-database-architecture.md)
9. [Multitenancy Architecture](09-multitenancy-architecture.md)
10. [API Contracts](10-api-contracts.md)
11. [Testing, Observability, And Security](11-testing-observability-security.md)
12. [Roadmap](12-roadmap.md)
13. [Reference Architecture Blueprint](13-reference-architecture-blueprint.md)

## Architecture Decisions

- [ADR 0001: .NET 8 LTS](adr/0001-dotnet-8-lts.md)
- [ADR 0002: Modular Monolith](adr/0002-modular-monolith.md)
- [ADR 0003: PostgreSQL Shared DB Tenancy](adr/0003-postgresql-shared-db-tenancy.md)
- [ADR 0004: Outbox First](adr/0004-outbox-first.md)
- [ADR 0005: Keycloak Identity Provider](adr/0005-keycloak-identity-provider.md)
- [ADR 0006: Milan Evently Blueprint](adr/0006-milan-evently-blueprint.md)

## Source References

These docs use the following references as guiding material:

- [.NET release and support policy](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support)
- [EF Core 8 release notes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew)
- [EF Core multi-tenancy](https://learn.microsoft.com/en-us/ef/core/miscellaneous/multitenancy)
- [EF Core global query filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
- [ASP.NET Core Swagger/OpenAPI for ASP.NET Core 8](https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-8.0)
- [ASP.NET Core hosted services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [ASP.NET Core authorization policies](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)
- [ASP.NET Core JWT bearer authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication)
- [Keycloak securing applications overview](https://www.keycloak.org/securing-apps/overview)
- [Keycloak JavaScript adapter](https://www.keycloak.org/securing-apps/javascript-adapter)
- [Keycloak Admin REST API](https://www.keycloak.org/docs-api/latest/rest-api/index.html)
- [Angular version compatibility](https://angular.dev/reference/versions)
- [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd)
- [jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture)
- [ardalis/CleanArchitecture](https://github.com/ardalis/CleanArchitecture)

Local architecture study:

- `D:\MilanJovanovic\modular-monolith-cource\Milan Jovanovic - Modular Monolith Architecture (The Ultimate Modular Monolith Blueprint) (6.2024)\code`
