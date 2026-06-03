# ReserveFlow: Routing and Layout Plan

This document maps out the navigation routes, layout containers, and guard conditions for the ReserveFlow Angular client.

---

## 1. Layout Organization
To ensure pages inherit their appropriate shell, layouts wrap around `<router-outlet>` blocks.

```text
+-----------------------------------------------------------+
|                        Layout Component                    |
|  +-----------------------------------------------------+  |
|  |                     Header / Sidebar                |  |
|  +-----------------------------------------------------+  |
|  |                     <router-outlet>                 |  |
|  |  +-----------------------------------------------+  |  |
|  |  |                  Active Route Page             |  |  |
|  |  +-----------------------------------------------+  |  |
|  +-----------------------------------------------------+  |
+-----------------------------------------------------------+
```

### Layout Definitions

* **`PublicLayout`:** Contains a broad search nav bar, category links, tenant switcher, and a centered container.
* **`AuthLayout`:** Compact, centered layout with branding for Keycloak token exchanges and profile lookups.
* **`PlatformAdminLayout`:** Left navigation panel focused on tenants and categories.
* **`TenantAdminLayout`:** Dashboard layout with sidebar navigation, notifications bell, and tenant context switcher.
* **`StaffLayout`:** Mobile-optimized layout with a bottom tab navigation menu.
* **`CustomerLayout`:** Simple profile dashboard structure for customers managing active reservations.

---

## 2. Route Definition Chart

| Route Path | Layout Wrapped | Guard Applied | Lazy-Loaded Module Reference |
| :--- | :--- | :--- | :--- |
| **`/`** | `PublicLayout` | None | `PublicHomeComponent` |
| **`/categories`** | `PublicLayout` | None | `CategoryListComponent` |
| **`/categories/:categorySlug`**| `PublicLayout` | None | `CategoryTenantsComponent` |
| **`/t/:tenantSlug`** | `PublicLayout` | `TenantExistGuard` | `TenantProfileComponent` |
| **`/t/:tenantSlug/services/:serviceId`**| `PublicLayout` | `TenantExistGuard` | `ServiceDetailComponent` |
| **`/t/:tenantSlug/book`** | `PublicLayout` | `TenantExistGuard` | `BookingWizardComponent` |
| **`/booking/:bookingCode`** | `PublicLayout` | None | `BookingSuccessComponent` |
| **`/booking/lookup`** | `PublicLayout` | None | `BookingLookupComponent` |
| **`/auth/login`** | `AuthLayout` | None | Keycloak Redirector |
| **`/auth/register`** | `AuthLayout` | None | Keycloak Redirector |
| **`/platform`** | `PlatformAdminLayout`| `AuthGuard`, `PlatformRoleGuard` | `PlatformDashboardComponent` |
| **`/platform/tenants`** | `PlatformAdminLayout`| `AuthGuard`, `PlatformRoleGuard` | `PlatformTenantListComponent` |
| **`/platform/tenants/create`**| `PlatformAdminLayout`| `AuthGuard`, `PlatformRoleGuard` | `PlatformTenantCreateComponent`|
| **`/platform/tenants/:id`** | `PlatformAdminLayout`| `AuthGuard`, `PlatformRoleGuard` | `PlatformTenantDetailComponent`|
| **`/platform/categories`** | `PlatformAdminLayout`| `AuthGuard`, `PlatformRoleGuard` | `PlatformCategoryListComponent`|
| **`/platform/audit-logs`** | `PlatformAdminLayout`| `AuthGuard`, `PlatformRoleGuard` | `PlatformAuditLogsComponent` |
| **`/platform/usage`** | `PlatformAdminLayout`| `AuthGuard`, `PlatformRoleGuard` | `PlatformUsageComponent` |
| **`/admin`** | `TenantAdminLayout` | `AuthGuard`, `TenantRoleGuard` | `TenantDashboardComponent` |
| **`/admin/services`** | `TenantAdminLayout` | `AuthGuard`, `TenantRoleGuard` | `AdminServiceCrudComponent` |
| **`/admin/staff`** | `TenantAdminLayout` | `AuthGuard`, `TenantRoleGuard` | `AdminStaffCrudComponent` |
| **`/admin/resources`** | `TenantAdminLayout` | `AuthGuard`, `TenantRoleGuard` | `AdminResourceCrudComponent` |
| **`/admin/working-hours`** | `TenantAdminLayout` | `AuthGuard`, `TenantRoleGuard` | `AdminWorkingHoursEditorComponent`|
| **`/admin/booking-policies`**| `TenantAdminLayout` | `AuthGuard`, `TenantRoleGuard` | `AdminBookingPoliciesComponent`|
| **`/admin/bookings`** | `TenantAdminLayout` | `AuthGuard`, `TenantRoleGuard` | `AdminBookingCalendarComponent`|
| **`/admin/reports`** | `TenantAdminLayout` | `AuthGuard`, `TenantRoleGuard` | `AdminReportsComponent` |
| **`/admin/settings`** | `TenantAdminLayout` | `AuthGuard`, `TenantRoleGuard` | `AdminSettingsComponent` |
| **`/staff/schedule`** | `StaffLayout` | `AuthGuard`, `StaffRoleGuard` | `StaffScheduleComponent` |
| **`/staff/bookings`** | `StaffLayout` | `AuthGuard`, `StaffRoleGuard` | `StaffBookingsListComponent` |
| **`/staff/unavailable-periods`**| `StaffLayout` | `AuthGuard`, `StaffRoleGuard` | `StaffUnavailablePeriodsComponent`|
| **`/customer/bookings`** | `CustomerLayout` | `AuthGuard` | `CustomerBookingsComponent` |
| **`/customer/profile`** | `CustomerLayout` | `AuthGuard` | `CustomerProfileComponent` |

---

## 3. Angular Standalone Code Implementation Example
Below is an example of the route structure implementation within `app.routes.ts`:

```typescript
import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { tenantExistGuard } from './core/guards/tenant-exist.guard';

export const routes: Routes = [
  // Public Section
  {
    path: '',
    loadComponent: () => import('./shared/layout/public-layout.component').then(m => m.PublicLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/public/marketplace/marketplace.component').then(m => m.MarketplaceComponent)
      },
      {
        path: 'categories',
        loadComponent: () => import('./features/public/marketplace/categories.component').then(m => m.CategoriesComponent)
      },
      {
        path: 't/:tenantSlug',
        canActivate: [tenantExistGuard],
        children: [
          {
            path: '',
            loadComponent: () => import('./features/public/tenant-profile/tenant-profile.component').then(m => m.TenantProfileComponent)
          },
          {
            path: 'book',
            loadComponent: () => import('./features/public/booking-wizard/booking-wizard.component').then(m => m.BookingWizardComponent)
          }
        ]
      },
      {
        path: 'booking/:bookingCode',
        loadComponent: () => import('./features/public/booking-success/booking-success.component').then(m => m.BookingSuccessComponent)
      },
      {
        path: 'booking/lookup',
        loadComponent: () => import('./features/public/booking-lookup/booking-lookup.component').then(m => m.BookingLookupComponent)
      }
    ]
  },

  // Platform Admin Section
  {
    path: 'platform',
    loadComponent: () => import('./shared/layout/platform-layout.component').then(m => m.PlatformLayoutComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['PlatformAdmin'] },
    loadChildren: () => import('./features/platform-admin/platform-admin.routes').then(m => m.platformAdminRoutes)
  },

  // Tenant Admin Section
  {
    path: 'admin',
    loadComponent: () => import('./shared/layout/tenant-admin-layout.component').then(m => m.TenantAdminLayoutComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['TenantAdmin'] },
    loadChildren: () => import('./features/tenant-admin/tenant-admin.routes').then(m => m.tenantAdminRoutes)
  },

  // Staff Section
  {
    path: 'staff',
    loadComponent: () => import('./shared/layout/staff-layout.component').then(m => m.StaffLayoutComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Staff'] },
    loadChildren: () => import('./features/staff/staff.routes').then(m => m.staffRoutes)
  },

  // Customer Portal Section
  {
    path: 'customer',
    loadComponent: () => import('./shared/layout/customer-layout.component').then(m => m.CustomerLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'bookings',
        loadComponent: () => import('./features/customer/bookings/customer-bookings.component').then(m => m.CustomerBookingsComponent)
      }
    ]
  },

  // Fallbacks
  {
    path: 'forbidden',
    loadComponent: () => import('./shared/ui/error/forbidden.component').then(m => m.ForbiddenComponent)
  },
  {
    path: '**',
    loadComponent: () => import('./shared/ui/error/not-found.component').then(m => m.NotFoundComponent)
  }
];
```
