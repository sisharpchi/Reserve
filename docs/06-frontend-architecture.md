# Frontend Architecture

## Baseline

The frontend is an Angular SPA. The exact Angular major version should be pinned during implementation to the latest stable release that is compatible with the chosen Node LTS and project tooling.

Frontend baseline:

- Angular SPA
- Standalone components
- Feature-based route groups
- Angular Signals and services for local state
- Reactive Forms
- HTTP interceptors
- Keycloak/OIDC login using Authorization Code + PKCE
- Permission guards
- Typed API client generated from OpenAPI or hand-written typed services for MVP
- Tailwind CSS or Angular Material with custom design tokens

## App Areas

```text
src/app/
  core/
    api/
    auth/
    config/
    guards/
    interceptors/
    permissions/
  shared/
    ui/
    forms/
    layout/
    utils/
  features/
    public/
    auth/
    customer-portal/
    tenant-admin/
    staff/
    platform-admin/
```

## Route Groups

Public marketplace:

- `/`
- `/categories`
- `/categories/:categorySlug`
- `/t/:tenantSlug`
- `/t/:tenantSlug/services/:serviceId`
- `/t/:tenantSlug/book`
- `/booking/:bookingCode`

Auth:

- `/login`
- `/register` redirects to Keycloak registration when enabled
- `/forgot-password` redirects to Keycloak account/password flow when enabled
- `/profile`

Tenant admin:

- `/admin`
- `/admin/services`
- `/admin/staff`
- `/admin/resources`
- `/admin/schedules`
- `/admin/bookings`
- `/admin/reports`
- `/admin/settings`
- `/admin/notifications`

Staff:

- `/staff`
- `/staff/schedule`
- `/staff/unavailable-periods`

Platform admin:

- `/platform`
- `/platform/tenants`
- `/platform/categories`
- `/platform/audit`
- `/platform/usage`

Customer portal:

- `/me/bookings`
- `/me/bookings/:id`

## Layouts

Public layout:

- top navigation
- category/tenant search
- tenant public pages
- booking wizard

Admin layout:

- sidebar
- top bar with current tenant/user
- page header
- table/filter area
- action buttons

Staff layout:

- schedule-focused navigation
- day/week view
- quick actions for complete/no-show

Platform layout:

- platform-level navigation
- tenant and category management
- audit and usage views

## State Management

Use Angular services with Signals for MVP state:

- `AuthService`: tokens, current user, refresh flow
- `KeycloakAuthService`: Keycloak/OIDC initialization, login redirect, logout redirect, token update
- `TenantContextService`: selected/current tenant for admin routes
- `PermissionService`: role and permission checks
- `BookingWizardStore`: selected tenant, service, date, staff/resource, slot, customer info
- `ToastService`: success/error notifications
- `LoadingStateService`: shared loading helpers where useful

Avoid global store complexity until there is a real cross-feature state problem.

## API Client

Preferred:

- Generate a typed client from OpenAPI once API contracts stabilize.

MVP fallback:

- Hand-written typed services per feature:
  - `PublicApiService`
  - `AuthApiService` for `/api/auth/me` and ReserveFlow-specific session context
  - `TenantAdminApiService`
  - `PlatformAdminApiService`
  - `StaffApiService`

All API calls go through a configured `HttpClient` with:

- base URL from runtime environment config
- auth token interceptor
- token update through Keycloak/OIDC client
- correlation id header
- normalized error handling

## Authorization In UI

Frontend permission checks improve UX, but backend authorization is the source of truth.

Use:

- route guards for protected areas
- menu filtering by permission
- directive/helper for action buttons
- access denied page for forbidden routes

The SPA must not store client secrets. It uses a public Keycloak client with exact redirect URIs, and the backend remains responsible for API authorization.

Example permissions:

- `Platform.Tenants.Manage`
- `Tenant.Services.Manage`
- `Tenant.Staff.Manage`
- `Tenant.Resources.Manage`
- `Bookings.ViewAll`
- `Bookings.CancelAny`
- `Bookings.CompleteAssigned`
- `Reports.View`

## Booking Wizard

Steps:

1. Tenant/service selection
2. Staff/resource preference
3. Date selection
4. Slot selection
5. Customer info
6. Review and confirm
7. Success page

The wizard stores only temporary UI state. Final booking validity is always rechecked by the backend.

## UX Requirements

Every API-backed screen needs:

- loading state
- empty state
- error state
- validation messages
- permission-aware actions
- mobile-safe layout for public flow

Admin screens should be dense and operational, not marketing-style pages. Public booking screens can be more friendly and guided.

## References

- [Angular version compatibility](https://angular.dev/reference/versions)
- [Keycloak JavaScript adapter](https://www.keycloak.org/securing-apps/javascript-adapter)
- [ASP.NET Core Swagger/OpenAPI for ASP.NET Core 8](https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-8.0)
