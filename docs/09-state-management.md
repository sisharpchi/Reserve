# ReserveFlow: State Management Plan

This document explains the state management strategy for ReserveFlow, leveraging Angular Signals to build reactive, high-performance local and application states without external state machines.

---

## 1. State Management Philosophy
ReserveFlow operates on a **decentralized, service-wrapped state model**:
* **Signals first:** Writable signals represent primitive reactive containers. Component UI binds directly to signals, eliminating manual subscription management and memory leaks.
* **Encapsulation:** State modifiers live within the service class. Components cannot write to state signals directly; they execute service methods that mutate state values.
* **Component-Local Scope:** Temporary UI states (e.g., table sort directions, modal open states, active wizard sub-steps) belong strictly in the component class. Global states (e.g., active user profile, tenant context) reside in core singletons.

---

## 2. Core Application States

### A. Auth & Current User State (`core/auth/auth.state.ts`)
Tracks Keycloak tokens, authenticating flags, and local user profiles.

```typescript
import { Injectable, computed, signal } from '@angular/core';

export interface UserProfile {
  id: string;
  email: string;
  name: string;
  roles: string[];
  permissions: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthState {
  // Private writable signals
  private _accessToken = signal<string | null>(null);
  private _userProfile = signal<UserProfile | null>(null);
  private _isAuthenticated = signal<boolean>(false);

  // Public readonly signals
  public accessToken = this._accessToken.asReadonly();
  public userProfile = this._userProfile.asReadonly();
  public isAuthenticated = this._isAuthenticated.asReadonly();

  // Computed signals
  public permissions = computed(() => this._userProfile()?.permissions ?? []);
  public roles = computed(() => this._userProfile()?.roles ?? []);

  public setAuthenticatedState(token: string, profile: UserProfile): void {
    this._accessToken.set(token);
    this._userProfile.set(profile);
    this._isAuthenticated.set(true);
  }

  public clearState(): void {
    this._accessToken.set(null);
    this._userProfile.set(null);
    this._isAuthenticated.set(false);
  }
}
```

### B. Tenant Context State (`core/tenant/tenant-context.state.ts`)
Maintains the active tenant context for database scope resolution.

```typescript
@Injectable({
  providedIn: 'root'
})
export class TenantContextState {
  private _currentTenantId = signal<string | null>(null);
  private _currentTenantSlug = signal<string | null>(null);
  private _timezone = signal<string>('UTC');

  public currentTenantId = this._currentTenantId.asReadonly();
  public currentTenantSlug = this._currentTenantSlug.asReadonly();
  public timezone = this._timezone.asReadonly();

  public setTenantContext(id: string, slug: string, tz: string): void {
    this._currentTenantId.set(id);
    this._currentTenantSlug.set(slug);
    this._timezone.set(tz);
  }

  public clearTenantContext(): void {
    this._currentTenantId.set(null);
    this._currentTenantSlug.set(null);
    this._timezone.set('UTC');
  }
}
```

### C. Booking Wizard State (`features/public/booking-wizard/booking-wizard.state.ts`)
Manages step-by-step selections of the public client booking process. Since the wizard is temporary, its service is provided directly at the `BookingWizardComponent` level rather than globally (recreating the state when starting a new booking).

```typescript
export interface WizardData {
  serviceId: string | null;
  staffId: string | null;      // null represents "Any Available"
  date: string | null;         // YYYY-MM-DD format
  slotTime: string | null;     // HH:MM format
  customerName: string;
  customerEmail: string;
  customerPhone: string;
}

@Injectable()
export class BookingWizardState {
  private _wizardData = signal<WizardData>({
    serviceId: null,
    staffId: null,
    date: null,
    slotTime: null,
    customerName: '',
    customerEmail: '',
    customerPhone: ''
  });

  public wizardData = this._wizardData.asReadonly();
  
  // Track step index
  private _currentStep = signal<number>(0);
  public currentStep = this._currentStep.asReadonly();

  public updateService(serviceId: string): void {
    this._wizardData.update(state => ({ ...state, serviceId }));
    this._currentStep.set(1);
  }

  public updateStaff(staffId: string | null): void {
    this._wizardData.update(state => ({ ...state, staffId }));
    this._currentStep.set(2);
  }

  public updateDateTime(date: string, slotTime: string): void {
    this._wizardData.update(state => ({ ...state, date, slotTime }));
    this._currentStep.set(3);
  }

  public setCustomerInfo(name: string, email: string, phone: string): void {
    this._wizardData.update(state => ({ 
      ...state, 
      customerName: name, 
      customerEmail: email, 
      customerPhone: phone 
    }));
    this._currentStep.set(4);
  }

  public prevStep(): void {
    if (this._currentStep() > 0) {
      this._currentStep.update(s => s - 1);
    }
  }

  public resetWizard(): void {
    this._wizardData.set({
      serviceId: null,
      staffId: null,
      date: null,
      slotTime: null,
      customerName: '',
      customerEmail: '',
      customerPhone: ''
    });
    this._currentStep.set(0);
  }
}
```

---

## 3. Local UI Filtering State Pattern
In admin pages, list filter parameters are signals to allow instant reactive tabular updates.

```typescript
@Component({
  selector: 'rf-bookings-list',
  standalone: true,
  template: `
    <input (input)="onSearchChange($event)" placeholder="Search bookings..." />
    <rf-table [data]="filteredBookings()" [loading]="isLoading()" />
  `
})
export class BookingsListComponent {
  private bookingsService = inject(BookingsApiService);

  // Filter signals
  public searchQuery = signal<string>('');
  public selectedStatus = signal<string>('All');
  public rawBookings = signal<Booking[]>([]);
  public isLoading = signal<boolean>(false);

  // Derived filter computation
  public filteredBookings = computed(() => {
    const query = this.searchQuery().toLowerCase();
    const status = this.selectedStatus();
    
    return this.rawBookings().filter(b => {
      const matchQuery = b.customerName.toLowerCase().includes(query) || b.id.includes(query);
      const matchStatus = status === 'All' || b.status === status;
      return matchQuery && matchStatus;
    });
  });

  public onSearchChange(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.searchQuery.set(val);
  }
}
```

---

## 4. When NOT to Overuse Global State
* **Do NOT put UI toggle states** (e.g., `isSidebarCollapsed`, `isFilterExpanded`, `themeMode`) in global states unless they must survive route transitions.
* **Do NOT store raw API lists** globally if they are only used on a single list view. Let the route components fetch and store that data locally within their life cycles, freeing memory when navigating away.
* **Only use global states** for shared context keys, session details, and shared UI primitives (e.g., toast arrays).
