# ReserveFlow: Security and Permissions Plan

This document outlines the security mechanisms implemented in the ReserveFlow frontend, ensuring role-based access, permission-based elements, tenant isolation, and integration with OIDC (Keycloak).

---

## 1. Core Security Principle: Zero Trust Frontend
* **Frontend security is UX only:** Shifting buttons, hiding menus, and blocking routes are purely usability controls.
* **Backend is the gatekeeper:** The backend composition layer intercepts every request, parses claims from verified JWTs, checks permissions, and validates tenant boundaries. The frontend must expect API responses to occasionally fail with `401 Unauthorized` or `403 Forbidden` and handle these states gracefully.

---

## 2. JWT and OIDC Keycloak Integration
The frontend delegates identity, registration, MFA, and credentials storage to Keycloak.

* **Keycloak JS Adapter:** Initialized at boot inside `app.config.ts` via an `APP_INITIALIZER`.
* **PKCE Flow:** Configured with public client IDs; client secrets are never exposed on the client.
* **Automatic Refresh Policies:** Before every API call (using HTTP interceptors), `keycloak.updateToken(30)` evaluates token life. If expired or expiring within 30 seconds, it fetches a fresh token silently.

---

## 3. Structural Directive: `*rfHasPermission`
We implement a custom structural directive to hide buttons and links that the active user lacks permissions to trigger.

```typescript
import { Directive, Input, TemplateRef, ViewContainerRef, inject, effect } from '@angular/core';
import { AuthState } from '../auth/auth.state';

@Directive({
  selector: '[rfHasPermission]',
  standalone: true
})
export class HasPermissionDirective {
  private authState = inject(AuthState);
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);

  private requiredPermission = '';
  private hasView = false;

  @Input() set rfHasPermission(permission: string) {
    this.requiredPermission = permission;
    this.updateView();
  }

  constructor() {
    // React to changes in user permissions signal automatically
    effect(() => {
      this.authState.permissions(); // register dependency
      this.updateView();
    });
  }

  private updateView(): void {
    const permissions = this.authState.permissions();
    const isAuthorized = permissions.includes(this.requiredPermission);

    if (isAuthorized && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!isAuthorized && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}
```

### Usage:
```html
<button *rfHasPermission="'Tenant.Services.Manage'" (click)="onDeleteService(id)" class="btn-danger">
  Delete Service
</button>
```

---

## 4. Route Guards (Auth, Role, Tenant Exist)

### A. Auth Guard (`core/guards/auth.guard.ts`)
Forces redirect to Keycloak login if the user attempts to enter protected routing tree nodes anonymously.
```typescript
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthState } from '../auth/auth.state';

export const authGuard: CanActivateFn = () => {
  const authState = inject(AuthState);
  const router = inject(Router);

  if (authState.isAuthenticated()) {
    return true;
  }

  // Store target URL to redirect back after success
  const target = window.location.pathname;
  localStorage.setItem('auth_redirect_target', target);
  
  // Trigger Keycloak login redirect
  router.navigate(['/auth/login']);
  return false;
};
```

### B. Role / Permission Guard (`core/guards/role.guard.ts`)
Validates that the logged-in user possesses the required application roles or specific module permissions.
```typescript
export const roleGuard: CanActivateFn = (route) => {
  const authState = inject(AuthState);
  const router = inject(Router);
  
  const requiredRoles = route.data['roles'] as string[];
  const userRoles = authState.roles();

  const hasAccess = requiredRoles.some(role => userRoles.includes(role));

  if (hasAccess) {
    return true;
  }

  router.navigate(['/forbidden']);
  return false;
};
```

---

## 5. Tenant Isolation & Suspended States

### Tenant Isolation in Routing
When navigating public page routing trees (e.g., `/t/barber-shop`), the route parameter `tenantSlug` is monitored.
* The `TenantExistGuard` validates the slug via a lightweight metadata API call: `GET /api/public/tenants/metadata?slug=barber-shop`.
* If the tenant does not exist or is suspended, the guard intercepts the flow.

### Handling Suspended Tenants
If the tenant metadata indicates a `Suspended` status:
* **The Interceptor Route:** The customer is immediately routed to `/t/suspended`. This page displays a clean, user-friendly suspension message: *"This business page is temporarily inactive. Please contact support."*
* **API Isolation:** The backend API immediately rejects requests with `403 Forbidden` if queries contain a header mapping to a suspended `TenantId`, ensuring absolute data lockdown.
