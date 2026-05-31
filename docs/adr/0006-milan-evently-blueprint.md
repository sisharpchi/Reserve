# ADR 0006: Use The Milan Evently Modular Monolith Blueprint

## Status

Accepted

## Context

ReserveFlow needs a concrete project shape before implementation starts. A vague "modular monolith" can still become a flat layered app, a folder-only monolith, or an over-designed distributed system.

The local Milan Jovanovic/Evently course code provides a useful .NET 8 modular monolith blueprint: common building blocks, per-module projects, API composition root, module config files, automatic endpoint registration, module schemas, outbox/inbox processors, Keycloak integration, strict build settings, and architecture tests.

ReserveFlow also uses ideas from kgrzybek's DDD modular monolith and the Jason Taylor/Ardalis Clean Architecture templates.

## Decision

Use the Evently-style structure as ReserveFlow's implementation baseline:

- `ReserveFlow.Common.Domain`
- `ReserveFlow.Common.Application`
- `ReserveFlow.Common.Infrastructure`
- `ReserveFlow.Common.Presentation`
- one API composition root
- per-module `Domain`, `Application`, `Infrastructure`, `IntegrationEvents`, and `Presentation` projects
- module-owned EF Core `DbContext`, schema, migrations, outbox, and inbox
- module endpoint registration by convention
- architecture tests for module boundaries and layer rules
- Keycloak integration through OIDC/JWT validation and backend-only Admin REST usage

This is an architectural pattern decision, not a decision to copy source code.

## Consequences

Positive:

- module navigation is predictable
- cross-module coupling is easier to detect
- API host remains thin
- module extraction remains possible later
- tests can enforce the architecture
- local development has a realistic infrastructure profile

Tradeoffs:

- more projects exist from the start
- initial solution setup takes longer
- developers must learn module registration, assembly references, and endpoint discovery
- CI must include architecture tests early

## References

- [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd)
- [jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture)
- [ardalis/CleanArchitecture](https://github.com/ardalis/CleanArchitecture)
- [Keycloak securing applications overview](https://www.keycloak.org/securing-apps/overview)
