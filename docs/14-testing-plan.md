# ReserveFlow: Testing Plan

This document defines the testing strategy, tools, and code patterns to verify the stability, security, and performance of the ReserveFlow Angular client.

---

## 1. Testing Pyramid & Tooling

```text
    +----------------------------------+
    |             E2E Tests            |  <- Cypress / Playwright (Later phase)
    +----------------------------------+
    |          Component Tests         |  <- Jasmine / Jest / Angular TestBed (P0)
    +----------------------------------+
    |            Unit Tests            |  <- Services, Guards, Interceptors (P0)
    +----------------------------------+
```

* **Unit Testing:** Focuses on pure services, route guards, HTTP interceptors, form validation rule maps, and utility functions.
* **Component Testing:** Validates DOM rendering, button clicks, user input flows, structural directives (`*rfHasPermission`), and reactive state emissions.
* **E2E Testing (Future):** Validates complete multi-role journeys (e.g., Customer books -> Tenant Admin approves).
* **Test Runner:** Driven by `npm run test` utilizing the default Angular test suite (Jasmine + Karma or Jest).

---

## 2. Guard Unit Test Code Example
Route guards represent critical security boundaries. Below is a unit test validating `AuthGuard` behavior:

```typescript
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthState } from '../auth/auth.state';

describe('authGuard', () => {
  let authStateMock: any;
  let routerMock: any;

  beforeEach(() => {
    authStateMock = {
      isAuthenticated: jasmine.createSpy('isAuthenticated')
    };

    routerMock = {
      navigate: jasmine.createSpy('navigate')
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthState, useValue: authStateMock },
        { provide: Router, useValue: routerMock }
      ]
    });
  });

  it('should allow navigation if user is authenticated', () => {
    authStateMock.isAuthenticated.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

    expect(result).toBeTrue();
    expect(routerMock.navigate).not.toHaveBeenCalled();
  });

  it('should block navigation and redirect to login if user is anonymous', () => {
    authStateMock.isAuthenticated.and.returnValue(false);

    const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

    expect(result).toBeFalse();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/auth/login']);
  });
});
```

---

## 3. Reactive Form Validation Unit Test
Testing form validation models verifies data constraints before hitting backend endpoints.

```typescript
import { FormControl, FormGroup, Validators } from '@angular/forms';

describe('Service CRUD Form Validation', () => {
  let form: FormGroup;

  beforeEach(() => {
    form = new FormGroup({
      name: new FormControl('', Validators.required),
      price: new FormControl(0, [Validators.required, Validators.min(0)]),
      duration: new FormControl(30, [Validators.required, Validators.min(15)])
    });
  });

  it('should mark form as invalid when empty', () => {
    expect(form.invalid).toBeTrue();
  });

  it('should flag name as required', () => {
    const nameCtrl = form.get('name')!;
    expect(nameCtrl.valid).toBeFalse();
    expect(nameCtrl.errors?.['required']).toBeTrue();
  });

  it('should block negative pricing values', () => {
    const priceCtrl = form.get('price')!;
    priceCtrl.setValue(-10);
    expect(priceCtrl.valid).toBeFalse();
    expect(priceCtrl.errors?.['min']).toBeDefined();
  });
});
```

---

## 4. Critical User Flows to Verify

### A. Authentication & Token Refresh Flow
* **Verification Scope:** Verify that expiring tokens trigger silent background OIDC updates. If Keycloak returns a `401 Unauthorized` due to a revoked session, ensure the app redirects to the login screen and clears all cached states.

### B. Public Booking Flow
* **Verification Scope:** Selecting a service, choosing a staff member, picking a date/time slot, and submitting client details.
* **Checks:** Skeletons appear during API queries; slots already booked are disabled; submitting a booking creates the appointment and loads the success screen with the confirmation code.

### C. Tenant Admin Service Creation
* **Verification Scope:** Admin configures a service with required resources and staff associations.
* **Checks:** Form inputs validate field data; selecting staff members updates database mapping; clicking submit shows a success toast notification.

### D. Staff Schedule Update
* **Verification Scope:** Practitioner updates their schedule and marks appointments.
* **Checks:** Calendar UI displays bookings color-coded by status; clicking "Mark Completed" updates the status; creating an unavailable period removes related slots from availability queries immediately.

### E. Platform Tenant Provisioning
* **Verification Scope:** Platform Admin creates and configures a tenant.
* **Checks:** Submitting the form initiates the provisioning call; suspending a tenant triggers the suspended screen on the tenant's public routes immediately.
