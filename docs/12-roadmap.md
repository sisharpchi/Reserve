# Roadmap

## Build Order

Do not build random features. Build the system in this order:

1. Foundation
2. Identity
3. Tenancy
4. Platform Admin
5. Tenant Admin Services, Staff, Resources
6. Scheduling Engine
7. Booking Core
8. Public Booking UI
9. Notifications And Outbox
10. Admin UI Polish
11. Testing, Security, Observability
12. Production Release

This order exists because booking depends on configured tenant, service, staff/resource, and schedule data.

Foundation means more than "empty API". It must establish the Evently-style architecture template: `Common.*` projects, module project shape, module config loader, endpoint discovery, strict build settings, health checks, Keycloak, PostgreSQL, and one sample module endpoint.

## MVP Release

MVP release should include:

- runnable API
- runnable frontend
- PostgreSQL Docker Compose setup
- Keycloak Docker Compose setup with development realm/client
- optional local Seq/Redis/Jaeger profiles documented
- seed demo tenant
- platform admin login through Keycloak
- tenant admin configuration flow
- public customer booking flow
- staff schedule/status flow
- outbox-backed notification/reminder flow
- module-owned outbox/inbox tables where needed
- integration/API tests for critical paths
- architecture tests for module and layer boundaries
- health checks and useful logs

## Advanced Features Later

Add after MVP:

- waitlist and automatic promotion
- external calendar sync
- payment integration
- webhook inbox
- tenant billing/subscription plans
- tenant branding
- localization
- read model projections
- caching for availability search
- advanced reporting
- no-show heuristics
- SQL Server provider option
- mobile app or PWA polish

## Release Checklist

Backend:

- migrations tested on clean database
- explicit `ReserveFlow.MigrationService` runner executed before API rollout
- seed demo tenant works
- Keycloak JWT validation flow tested
- tenant isolation tested
- booking overlap prevention tested
- outbox processor tested
- reminder flow tested
- health checks work
- structured logs include correlation id and tenant id
- rate limiting enabled
- CORS configured
- secrets not committed
- Docker image builds

Frontend:

- production build passes
- public booking flow tested
- tenant admin flow tested
- platform admin flow tested
- staff flow tested
- Keycloak/OIDC token update works
- permission-based menus work
- mobile layout acceptable
- empty/loading/error states present
- runtime API URL configured

Product:

- demo tenant created
- demo services created
- demo staff/resources created
- demo working hours configured
- customer booking works
- admin can manage booking
- staff can complete/no-show booking
- reminder appears in outbox/notification flow
- README updated
- screenshots added

## Documentation Maintenance

Docs should be updated when:

- endpoint contracts change
- module boundaries change
- database schema changes meaningfully
- tenant isolation rules change
- MVP scope changes
- deployment strategy changes

Docs are part of the product, not a one-time artifact.
