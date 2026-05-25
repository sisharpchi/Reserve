# Product System Design

## Product Idea

ReserveFlow is a SaaS booking platform where different businesses can publish their services, working hours, staff, resources, and booking rules.

In real life, each business is a tenant:

- Smile Dental Clinic
- Elite Barber Shop
- WorkHub Coworking
- IELTS Master Academy
- Consulting agency

Customers visit the platform, choose a tenant, select a service, find an available slot, and create a booking.

The important part is that scheduling is not only CRUD. Real booking systems need working hours, staff availability, resource calendars, cancellation windows, reminders, no-show handling, tenant isolation, admin roles, reporting, and audit history.

## Real-World Problem

Small booking apps often start with a simple `CreateBooking` endpoint. That breaks down when the business asks for real rules:

- A doctor works Monday to Friday from 09:00 to 18:00.
- Lunch time is 13:00 to 14:00.
- A service can take 30, 45, 60, or 90 minutes.
- A room, chair, or meeting room cannot be booked twice at the same time.
- A customer should not cancel after the cancellation deadline.
- Staff can mark a booking as completed or no-show.
- Tenant admins must not see another tenant's data.
- Platform admins manage tenants, not daily appointments.
- Notifications must be reliable even if the app restarts.

ReserveFlow solves this as a real backend engineering problem: transactional booking logic, tenant isolation, role-based administration, scheduling rules, background work, and operational visibility.

## MVP Goal

The MVP proves the full booking loop:

1. Platform admin creates a tenant and category.
2. Tenant admin configures services, staff, resources, schedules, and policies.
3. Customer browses tenants and services.
4. Customer searches availability.
5. Customer creates a booking.
6. Booking is protected from staff/resource overlap.
7. Confirmation and reminder events go through outbox.
8. Staff or tenant admin manages booking status.

## MVP Scope

Included:

- Tenant and category management
- Keycloak-backed authentication, app roles, permissions, and tenant membership
- Tenant public profile
- Service catalog
- Staff and resources
- Working hours and unavailable periods
- Availability search
- Booking create, cancel, reschedule, complete, no-show
- Notification abstraction and fake sender
- Outbox processor using ASP.NET Core `BackgroundService`
- PostgreSQL with EF Core 8.x migrations
- OpenAPI using Swashbuckle or NSwag for ASP.NET Core 8
- Keycloak local container/realm setup for development
- Unit, integration, API, and architecture tests
- Docker Compose for local development
- Health checks, structured logs, and OpenTelemetry-ready telemetry

Not included in MVP:

- Payments
- External calendar sync
- SMS provider integration
- Waitlist optimization
- Advanced analytics
- Tenant-specific mobile apps
- Tenant-per-database or tenant-per-schema isolation
- Full subscription billing

## Success Criteria

The project is successful when a demo tenant can be configured from the admin UI and a customer can complete a real booking flow without manual database changes.

The most important technical success criteria are:

- Cross-tenant data access is blocked.
- Staff and resources cannot be double-booked.
- Booking lifecycle rules are enforced in domain/application logic.
- Availability search returns tenant-local times correctly.
- Outbox messages are saved transactionally with booking changes.
- The API and frontend have clear failure, loading, and empty states.

## References

- [.NET release and support policy](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support)
- [EF Core 8 release notes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew)
- [Keycloak securing applications overview](https://www.keycloak.org/securing-apps/overview)
- [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd)
