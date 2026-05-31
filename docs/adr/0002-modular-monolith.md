# ADR 0002: Use A Modular Monolith

## Status

Accepted

## Context

ReserveFlow has meaningful business complexity: staff calendars, resource calendars, tenant isolation, booking rules, reminders, audit, reporting, and external integrations later.

Starting with microservices would add broker, deployment, service discovery, distributed tracing, distributed data consistency, and operational complexity before the domain is stable.

## Decision

Use a modular monolith:

- one deployable backend application for MVP
- clear bounded contexts/modules
- Clean Architecture inside modules
- vertical slices per use case
- per-module schemas in one PostgreSQL database
- Milan Jovanovic/Evently-style `Common.*` projects and per-module `Domain/Application/Infrastructure/IntegrationEvents/Presentation` projects
- API composition root that references module infrastructure projects and maps module presentation endpoints
- asynchronous-style integration through events/outbox where useful

## Consequences

Positive:

- easy local development and debugging
- clear module boundaries
- avoids premature distributed complexity
- can split worker from API later
- some modules can be extracted later if there is a real need

Tradeoffs:

- discipline is required to avoid direct cross-module coupling
- one deployment means modules do not scale independently in MVP
- cross-schema reads must be reviewed carefully
- more projects exist up front, so module templates and architecture tests matter

## References

- [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd)
- [jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture)
- [ardalis/CleanArchitecture](https://github.com/ardalis/CleanArchitecture)
