# Frontend Implementation Plan

## FE-M0 Foundation

Goal: create a runnable Angular app foundation.

Tasks:

- Create Angular workspace.
- Configure public, auth, tenant admin, staff, and platform admin routes.
- Add app layouts.
- Add runtime API URL config.
- Add HTTP interceptor skeleton for bearer token attachment.
- Add Keycloak client configuration placeholders.
- Add base UI components: button, input, select, dialog, table, badge, empty state, loading state.
- Add Docker/Nginx setup for production static hosting.

Acceptance:

- App runs locally.
- Routes load placeholder pages.
- API base URL can be changed without rebuilding the image.

## FE-M1 Auth

Goal: users can log in and enter allowed areas.

Tasks:

- Login route that redirects to Keycloak.
- Auth service backed by Keycloak/OIDC.
- `/api/auth/me` integration for ReserveFlow user, tenant, and permission context.
- Route guards.
- Permission helper/directive.
- Profile page.
- Logout flow through Keycloak.
- Access denied page.

Acceptance:

- Unauthorized users are redirected to login.
- Menus and buttons respect permissions.
- Access token update/refresh works through the Keycloak/OIDC client before API calls when possible.

## FE-M2 Platform Admin

Goal: platform admin can manage global platform data.

Tasks:

- Platform dashboard.
- Tenant list with search, status filter, pagination.
- Create tenant form.
- Edit tenant/status page.
- Assign tenant owner flow.
- Category management.
- Platform audit log page.

Acceptance:

- Platform admin can create a demo tenant from UI.
- Suspended/active status is visible and manageable.

## FE-M3 Tenant Admin Core

Goal: tenant admin can configure services, staff, resources, and policies.

Tasks:

- Tenant dashboard.
- Services list and service form.
- Staff list and staff form.
- Staff working hours editor.
- Staff unavailable periods editor.
- Resources list and resource form.
- Resource working hours editor.
- Booking policy settings.
- Tenant profile settings.

Acceptance:

- Tenant admin can configure all data needed for availability search.
- Forms have validation and clear error messages.

## FE-M4 Availability UI

Goal: public flow can display correct slot options.

Tasks:

- Service details page.
- Staff/resource selector.
- Date picker.
- Slot picker grouped by staff/resource.
- Empty state when no slots exist.
- Loading/error states.
- Tenant-local timezone display.

Acceptance:

- Customer can choose a date and see available slots.
- "Any available" mode is understandable.

## FE-M5 Booking UI

Goal: customer and admin can manage booking lifecycle.

Tasks:

- Public booking form.
- Booking review and confirmation.
- Booking success page.
- Booking lookup page.
- Public cancel/reschedule flow.
- Tenant admin bookings table.
- Tenant admin booking details.
- Admin cancel/reschedule actions.
- Staff schedule page.
- Staff complete/no-show actions.

Acceptance:

- Customer can create, view, cancel, and reschedule booking when policy allows.
- Tenant admin can manage bookings in own tenant.
- Staff can update assigned booking status.

## FE-M6 Notifications And Logs

Goal: tenant admin can inspect notification state.

Tasks:

- Notification settings placeholder.
- Notification logs page.
- Failed notification/outbox view.
- Retry failed message action when backend exposes it.

Acceptance:

- Tenant admin can see whether notifications were sent or failed.

## FE-M7 Public Marketplace

Goal: public side feels like a real booking platform.

Tasks:

- Home page with category/search entry.
- Category tenants page.
- Tenant public profile page.
- Active services list.
- Booking wizard polish.
- Responsive pass for mobile.
- Public SEO metadata where the chosen frontend rendering setup supports it.

Acceptance:

- New customer can move from homepage to confirmed booking without admin help.

## FE-M8 Production Polish

Goal: admin UI is consistent and maintainable.

Tasks:

- Reusable table patterns.
- Filter/sort/pagination consistency.
- Toast notifications.
- Confirmation dialogs for dangerous actions.
- Loading skeletons.
- Breadcrumbs.
- Audit log UI.
- Dashboard charts.

Acceptance:

- Main admin workflows feel consistent.
- No major console errors.

## FE-M9 Hardening

Goal: production build and critical UI tests are reliable.

Tasks:

- Production build config.
- Basic component/service tests.
- Auth refresh reliability tests.
- Accessibility pass for forms and buttons.
- Bundle review and lazy route check.

Acceptance:

- Production build passes.
- Public and admin flows are usable on mobile and desktop.

## FE-M10 Release

Goal: demo-ready frontend.

Tasks:

- Production Dockerfile/Nginx image.
- Runtime config docs.
- Demo walkthrough pages or route notes.
- README screenshots.
- Final responsive QA.

Acceptance:

- Frontend container serves the app.
- Demo tenant flow is easy to show.
