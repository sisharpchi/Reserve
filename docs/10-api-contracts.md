# API Contracts

## API Principles

- Endpoints are thin and delegate to application slices.
- Endpoint classes live in module `Presentation` projects and are discovered by the API host.
- Public tenant routes use `tenantSlug`.
- Tenant admin and staff routes require authentication and tenant membership.
- Platform routes require platform permissions.
- Authentication is delegated to Keycloak; ReserveFlow APIs validate bearer tokens and expose only application context endpoints.
- Write endpoints return stable identifiers and structured errors.
- All date/time values crossing API boundaries use ISO 8601.
- Persisted booking times are UTC; UI displays tenant-local times.

## Public APIs

```http
GET    /api/public/categories
GET    /api/public/tenants?categoryId=&search=
GET    /api/public/tenants/{tenantSlug}
GET    /api/public/tenants/{tenantSlug}/services
GET    /api/public/tenants/{tenantSlug}/services/{serviceId}
GET    /api/public/tenants/{tenantSlug}/availability
POST   /api/public/tenants/{tenantSlug}/bookings
GET    /api/public/tenants/{tenantSlug}/bookings/{bookingCode}
POST   /api/public/tenants/{tenantSlug}/bookings/{bookingCode}/cancel
POST   /api/public/tenants/{tenantSlug}/bookings/{bookingCode}/reschedule
```

Availability query parameters:

```text
serviceId
date
staffMemberId optional
resourceId optional
mode optional: specific | any
```

Create booking request:

```json
{
  "serviceId": "uuid",
  "staffMemberId": "uuid-or-null",
  "resourceId": "uuid-or-null",
  "startsAtLocal": "2026-06-01T10:30:00",
  "customer": {
    "fullName": "Ali Valiyev",
    "email": "ali@example.com",
    "phone": "+998901234567"
  },
  "idempotencyKey": "client-generated-key"
}
```

## Auth And User Context APIs

```http
GET    /api/auth/me
POST   /api/auth/sync-user
```

Login, registration, password reset, refresh, and logout happen through Keycloak/OIDC endpoints, not ReserveFlow password endpoints.

Current user response:

```json
{
  "user": {
    "id": "uuid",
    "keycloakSubject": "keycloak-sub",
    "email": "admin@example.com",
    "roles": ["TenantAdmin"],
    "permissions": ["Tenant.Services.Manage"],
    "tenants": [
      {
        "tenantId": "uuid",
        "slug": "smile-dental",
        "role": "TenantAdmin"
      }
    ]
  },
  "platformPermissions": ["Platform.Tenants.Manage"]
}
```

`POST /api/auth/sync-user` creates or updates the local ReserveFlow user profile from validated Keycloak claims. It does not create passwords or issue tokens.

Admin-only provisioning endpoints call Keycloak Admin REST API from backend code. The Angular frontend must never hold Keycloak admin credentials.

## Platform Admin APIs

```http
GET    /api/platform/categories
POST   /api/platform/categories
PUT    /api/platform/categories/{id}

GET    /api/platform/tenants
POST   /api/platform/tenants
GET    /api/platform/tenants/{id}
PUT    /api/platform/tenants/{id}
POST   /api/platform/tenants/{id}/activate
POST   /api/platform/tenants/{id}/suspend
POST   /api/platform/tenants/{id}/assign-owner
POST   /api/platform/tenants/{id}/owner/invite

GET    /api/platform/usage
GET    /api/platform/audit-logs?pageNumber=1&pageSize=20&tenantId={tenantId}&action=TenantSuspended&entityName=Tenant&fromUtc=2026-06-01T00:00:00Z&toUtc=2026-06-30T23:59:59Z&sortBy=occurredOnUtc&sortDirection=desc
```

`POST /api/platform/tenants/{id}/assign-owner` is the manual fallback when a Keycloak subject is already known.

`POST /api/platform/tenants/{id}/owner/invite` is the normal backend provisioning flow. It:

- uses the backend-only Keycloak Admin client credentials
- finds or creates the Keycloak user by email
- maps the Keycloak `tenant-admin` realm role
- optionally sends Keycloak required-action email when SMTP/config is enabled
- creates or updates local `identity.users`
- creates or updates local `identity.tenant_users` with role `TenantAdmin`

Example request:

```json
{
  "email": "owner@smileclinic.example",
  "displayName": "Smile Clinic Owner"
}
```

Example response:

```json
{
  "tenantId": "uuid",
  "userId": "uuid",
  "keycloakSubject": "keycloak-user-id",
  "email": "owner@smileclinic.example",
  "displayName": "Smile Clinic Owner",
  "role": "TenantAdmin",
  "createdInKeycloak": true,
  "invitationEmailSent": false,
  "localUserCreated": true,
  "membershipCreated": true
}
```

Required permissions:

- `Platform.Categories.Manage`
- `Platform.Tenants.Manage`
- `Platform.Audit.View`
- `Platform.Usage.View`

Platform audit logs return the same paged response shape as admin tables. Filters are optional:

- Audit logs: `tenantId`, `action`, `entityName`, `fromUtc`, `toUtc`, `sortBy=occurredOnUtc|tenantId|action|entityName`, `sortDirection=asc|desc`.

## Tenant Admin APIs

```http
GET    /api/admin/services?pageNumber=1&pageSize=20&search=dental&isActive=true&sortBy=name&sortDirection=asc
POST   /api/admin/services
GET    /api/admin/services/{id}
PUT    /api/admin/services/{id}
DELETE /api/admin/services/{id}

GET    /api/admin/staff?pageNumber=1&pageSize=20&search=ali&isActive=true&sortBy=displayName&sortDirection=asc
POST   /api/admin/staff
GET    /api/admin/staff/{id}
PUT    /api/admin/staff/{id}
POST   /api/admin/staff/{id}/working-hours
POST   /api/admin/staff/{id}/unavailable-periods

GET    /api/admin/resources?pageNumber=1&pageSize=20&search=room&resourceType=room&isActive=true&sortBy=name&sortDirection=asc
POST   /api/admin/resources
GET    /api/admin/resources/{id}
PUT    /api/admin/resources/{id}
POST   /api/admin/resources/{id}/working-hours
POST   /api/admin/resources/{id}/unavailable-periods

GET    /api/admin/bookings?pageNumber=1&pageSize=20&status=Confirmed&fromUtc=2026-06-01T00:00:00Z&toUtc=2026-06-30T23:59:59Z&sortBy=startsAtUtc&sortDirection=desc
GET    /api/admin/bookings/{id}
POST   /api/admin/bookings/{id}/cancel
POST   /api/admin/bookings/{id}/reschedule
POST   /api/admin/bookings/{id}/complete
POST   /api/admin/bookings/{id}/no-show

GET    /api/admin/reports/daily
GET    /api/admin/reports/staff-utilization
GET    /api/admin/reports/no-show
GET    /api/admin/notifications?pageNumber=1&pageSize=20&status=Pending&channel=Email&search=customer@example.com&sortBy=createdAtUtc&sortDirection=desc
GET    /api/admin/module-messages?pageNumber=1&pageSize=20&status=Failed&type=BookingCreated&sortBy=occurredOnUtc&sortDirection=desc
```

The main admin table endpoints return a paged response:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 0,
  "totalPages": 0,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

`pageSize` is capped by the backend at `100` for MVP admin screens. List filters are optional and always remain scoped to the current tenant:

- Services: `search`, `isActive`, `sortBy=name|durationMinutes|price|createdAtUtc`, `sortDirection=asc|desc`.
- Staff: `search`, `isActive`, `sortBy=displayName|email|createdAtUtc`, `sortDirection=asc|desc`.
- Resources: `search`, `resourceType`, `isActive`, `sortBy=name|resourceType|capacity|createdAtUtc`, `sortDirection=asc|desc`.
- Bookings: `status`, `fromUtc`, `toUtc`, `sortBy=startsAtUtc|endsAtUtc|status`, `sortDirection=asc|desc`.
- Notifications: `status`, `channel`, `search`, `sortBy=createdAtUtc|deliverAtUtc|sentAtUtc|status|channel`, `sortDirection=asc|desc`.
- Module messages: `status=Pending|Processed|Failed`, `type`, `sortBy=occurredOnUtc|processedOnUtc|type|status|retryCount`, `sortDirection=asc|desc`.

Required permissions:

- `Tenant.Services.Manage`
- `Tenant.Staff.Manage`
- `Tenant.Resources.Manage`
- `Tenant.Schedule.Manage`
- `Bookings.ViewAll`
- `Bookings.CancelAny`
- `Reports.View`
- `Notifications.View`

## Staff APIs

```http
GET    /api/staff/me/schedule
POST   /api/staff/bookings/{id}/complete
POST   /api/staff/bookings/{id}/no-show
POST   /api/staff/unavailable-periods
```

Required permissions:

- `Staff.Schedule.ViewOwn`
- `Bookings.CompleteAssigned`
- `Bookings.MarkNoShowAssigned`
- `Staff.Unavailable.ManageOwn`

## Error Shape

Use ProblemDetails style responses:

```json
{
  "type": "https://reserveflow/errors/validation",
  "title": "Validation failed",
  "status": 400,
  "traceId": "trace-id",
  "errors": {
    "serviceId": ["Service is required."]
  }
}
```

## Versioning

MVP starts without public URL versioning. If external partners start integrating, introduce `/api/v1/...` before publishing stable partner contracts.

## References

- [ASP.NET Core Swagger/OpenAPI for ASP.NET Core 8](https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-8.0)
- [ASP.NET Core authorization policies](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)
- [ASP.NET Core JWT bearer authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication)
- [Keycloak securing applications overview](https://www.keycloak.org/securing-apps/overview)
