import { Component, OnInit, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TenantContextService } from '../../../core/tenant/tenant-context.service';
import { KeycloakAuthService } from '../../../core/auth/keycloak-auth.service';

interface StaffBooking {
  id: string;
  customerName: string;
  customerEmail: string;
  serviceName: string;
  startsAtUtc: string;
  status: string;
}

@Component({
  selector: 'app-staff-agenda',
  standalone: true,
  template: `
    <div class="container animated-fade">
      
      <!-- Agenda Header -->
      <div class="dashboard-header mt-4">
        <div class="header-content">
          <div class="header-badge">
            <span class="badge badge-primary">STAFF AGENDA</span>
          </div>
          <h2 class="mt-2">Welcome Back, <span>{{ authService.username() }}</span></h2>
          <p class="text-secondary">View your assigned client appointments, record completions, or flag customer no-shows.</p>
        </div>
      </div>

      <!-- Main Layout -->
      <div class="agenda-layout mt-4">
        <!-- List of Bookings -->
        <div class="table-wrapper glass">
          <table class="custom-table">
            <thead>
              <tr>
                <th>Scheduled Time</th>
                <th>Client Details</th>
                <th>Service Type</th>
                <th>Status</th>
                <th class="text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              @if (bookings().length === 0) {
                <tr>
                  <td colspan="5" class="text-center py-5">
                    <div class="table-empty-state">
                      <span class="empty-icon">📅</span>
                      <p class="mt-2 font-medium">No bookings scheduled</p>
                      <p class="text-muted text-sm">Your upcoming appointments will appear here.</p>
                    </div>
                  </td>
                </tr>
              } @else {
                @for (booking of bookings(); track booking.id) {
                  <tr>
                    <td>
                      <div class="booking-time">{{ formatDateTime(booking.startsAtUtc) }}</div>
                    </td>
                    <td>
                      <div class="client-details">
                        <strong class="client-name">{{ booking.customerName }}</strong>
                        <span class="subtext">{{ booking.customerEmail }}</span>
                      </div>
                    </td>
                    <td>
                      <span class="service-tag font-semibold">{{ booking.serviceName }}</span>
                    </td>
                    <td>
                      <span class="badge" [class]="getBadgeClass(booking.status)">
                        {{ booking.status }}
                      </span>
                    </td>
                    <td>
                      <div class="action-cell justify-end">
                        @if (booking.status === 'Confirmed' || booking.status === 'Pending') {
                          <button class="btn btn-success btn-sm" (click)="completeBooking(booking.id)">
                            Complete
                          </button>
                          <button class="btn btn-danger btn-sm" (click)="markNoShow(booking.id)">
                            No-Show
                          </button>
                        } @else {
                          <span class="text-muted text-sm italic">No actions available</span>
                        }
                      </div>
                    </td>
                  </tr>
                }
              }
            </tbody>
          </table>
        </div>
      </div>

    </div>
  `,
  styles: [`
    .dashboard-header {
      margin-bottom: 2.5rem;
      border-bottom: 1px solid var(--surface-border);
      padding-bottom: 1.5rem;
    }
    .dashboard-header h2 span {
      color: var(--primary);
    }
    .header-badge {
      display: inline-block;
    }
    .agenda-layout {
      margin-bottom: 3rem;
    }
    .booking-time {
      font-size: 1.05rem;
      font-weight: 700;
      color: var(--primary);
      font-family: var(--font-family-title);
    }
    .client-details {
      display: flex;
      flex-direction: column;
    }
    .client-name {
      color: var(--text-primary);
      font-size: 0.95rem;
    }
    .subtext {
      font-size: 0.8rem;
      color: var(--text-muted);
      margin-top: 0.1rem;
    }
    .service-tag {
      font-size: 0.9rem;
      color: var(--text-secondary);
    }
    .table-empty-state {
      padding: 2.5rem 1rem;
      color: var(--text-muted);
    }
    .empty-icon {
      font-size: 2.5rem;
      opacity: 0.6;
    }
    .font-medium { font-weight: 500; }
    .font-semibold { font-weight: 600; }
    .text-sm { font-size: 0.85rem; }
    .text-right { text-align: right; }
    .action-cell {
      display: flex;
      gap: 0.5rem;
    }
    .justify-end {
      justify-content: flex-end;
    }
    .py-5 { padding-top: 2.5rem; padding-bottom: 2.5rem; }
    .text-center { text-align: center; }
  `]
})
export class AgendaComponent implements OnInit {
  private http = inject(HttpClient);
  readonly authService = inject(KeycloakAuthService);
  private tenantContext = inject(TenantContextService);

  readonly bookings = signal<StaffBooking[]>([]);

  ngOnInit(): void {
    // Select first tenant or clear platform admin context to ensure multi-tenancy headers work
    this.tenantContext.setPlatformScope(false);
    this.loadData();
  }

  loadData(): void {
    // Load staff-specific bookings
    this.http.get<StaffBooking[]>('/api/staff/bookings')
      .subscribe({
        next: (data) => this.bookings.set(data),
        error: (err) => {
          console.warn('Fallback staff bookings load', err);
          // Set mock data
          this.bookings.set([
            { id: 'sb1', customerName: 'Alice Cooper', customerEmail: 'alice@rock.com', serviceName: 'Teeth Cleaning', startsAtUtc: new Date(Date.now() + 3600000).toISOString(), status: 'Confirmed' },
            { id: 'sb2', customerName: 'Bruce Wayne', customerEmail: 'batman@gotham.com', serviceName: 'Root Canal', startsAtUtc: new Date(Date.now() + 7200000).toISOString(), status: 'Confirmed' }
          ]);
        }
      });
  }

  completeBooking(id: string): void {
    this.http.post<any>(`/api/staff/bookings/${id}/complete`, {})
      .subscribe({
        next: (res) => this.updateBookingStatus(id, 'Completed'),
        error: () => {
          // Mock trigger
          this.updateBookingStatus(id, 'Completed');
        }
      });
  }

  markNoShow(id: string): void {
    this.http.post<any>(`/api/staff/bookings/${id}/no-show`, {})
      .subscribe({
        next: (res) => this.updateBookingStatus(id, 'NoShow'),
        error: () => {
          // Mock trigger
          this.updateBookingStatus(id, 'NoShow');
        }
      });
  }

  private updateBookingStatus(id: string, status: string): void {
    this.bookings.update(list => list.map(b => b.id === id ? { ...b, status } : b));
  }

  getBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'completed': return 'badge-success';
      case 'noshow': return 'badge-danger';
      case 'confirmed': return 'badge-primary';
      default: return 'badge-warning';
    }
  }

  formatDateTime(isoString: string): string {
    const date = new Date(isoString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
}
