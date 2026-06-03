import { Routes } from '@angular/router';
import { MarketplaceComponent } from './features/public/marketplace/marketplace.component';
import { BookingWizardComponent } from './features/public/booking-wizard/booking-wizard.component';
import { DashboardComponent } from './features/tenant-admin/dashboard/dashboard.component';
import { TenantsComponent } from './features/platform-admin/tenants/tenants.component';
import { AgendaComponent } from './features/staff/agenda/agenda.component';
import { AccessDeniedComponent } from './features/auth/access-denied.component';
import { ProfileComponent } from './features/auth/profile.component';
import { BookingDetailComponent } from './features/public/booking-detail/booking-detail.component';
import { TenantProfileComponent } from './features/public/tenant-profile/tenant-profile.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    component: MarketplaceComponent
  },
  {
    path: 't/:tenantSlug',
    component: TenantProfileComponent
  },
  {
    path: 't/:tenantSlug/book',
    component: BookingWizardComponent
  },
  {
    path: 'booking',
    component: BookingDetailComponent
  },
  {
    path: 'booking/:bookingCode',
    component: BookingDetailComponent
  },
  {
    path: 'profile',
    component: ProfileComponent,
    canActivate: [authGuard([])]
  },
  {
    path: 'admin',
    component: DashboardComponent,
    canActivate: [authGuard(['tenant-admin'])]
  },
  {
    path: 'platform',
    component: TenantsComponent,
    canActivate: [authGuard(['platform-admin', 'system-admin'])]
  },
  {
    path: 'staff',
    component: AgendaComponent,
    canActivate: [authGuard(['staff'])]
  },
  {
    path: 'access-denied',
    component: AccessDeniedComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];
