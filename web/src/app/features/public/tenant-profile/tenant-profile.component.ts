import { Component, OnInit, signal, inject } from '@angular/core';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { DecimalPipe } from '@angular/common';
import { TenantContextService } from '../../../core/tenant/tenant-context.service';

interface TenantDetail {
  id: string;
  name: string;
  slug: string;
  timeZoneId: string;
  categoryId?: string;
  status: string;
}

interface Service {
  id: string;
  name: string;
  durationMinutes: number;
  price: number;
  currency: string;
}

interface Staff {
  id: string;
  displayName: string;
  email: string;
}

@Component({
  selector: 'app-tenant-profile',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
  template: `
    <div class="container animated-fade">
      @if (loading()) {
        <div class="loading-spinner-wrapper">
          <div class="spinner"></div>
          <p>Loading provider profile...</p>
        </div>
      } @else if (tenant(); as t) {
        <div class="profile-layout">
          
          <!-- Banner Hero -->
          <header class="hero-header glass">
            <div class="hero-content">
              <a routerLink="/" class="back-link">← Back to Marketplace</a>
              <h1 class="mt-2">{{ t.name }}</h1>
              <p class="text-secondary mt-1">
                Timezone Context: <strong>{{ t.timeZoneId }}</strong> 
                <span class="dot-separator">•</span> 
                Provider Status: <span class="badge badge-success">{{ t.status }}</span>
              </p>
            </div>
            <div class="hero-actions">
              <button 
                [routerLink]="['/t', t.slug, 'book']" 
                class="btn btn-primary"
                (click)="selectTenant(t)">
                📅 Book Appointment Now
              </button>
            </div>
          </header>

          <div class="grid-2 mt-4" style="margin-top: 2rem;">
            <!-- Left Column: Service Menu -->
            <div class="glass-card">
              <h3 class="border-bottom">Catalog Service Menu</h3>
              @if (services().length === 0) {
                <div class="py-4 text-center text-muted">No services registered by this provider.</div>
              } @else {
                <div class="menu-list mt-3">
                  @for (svc of services(); track svc.id) {
                    <div class="menu-item">
                      <div class="menu-details">
                        <h4>{{ svc.name }}</h4>
                        <div class="menu-meta">
                          <span class="badge badge-primary" style="font-size: 0.7rem; padding: 0.15rem 0.5rem;">⏱️ {{ svc.durationMinutes }} mins</span>
                        </div>
                      </div>
                      <span class="price-highlight">{{ svc.price | number }} {{ svc.currency }}</span>
                    </div>
                  }
                </div>
              }
            </div>

            <!-- Right Column: Staff Members -->
            <div class="glass-card">
              <h3 class="border-bottom">Our Professional Staff</h3>
              @if (staffList().length === 0) {
                <div class="py-4 text-center text-muted">No staff members listed.</div>
              } @else {
                <div class="staff-list mt-3">
                  @for (stf of staffList(); track stf.id) {
                    <div class="staff-card glass">
                      <div class="staff-avatar">👤</div>
                      <div class="staff-details">
                        <h4>{{ stf.displayName }}</h4>
                        <p class="staff-email">{{ stf.email }}</p>
                      </div>
                    </div>
                  }
                </div>
              }
            </div>
          </div>

        </div>
      } @else {
        <div class="empty-state-card mt-4">
          <span class="empty-state-icon">🚫</span>
          <h3>Provider profile not found</h3>
          <p>The requested booking workspace does not exist or has been deactivated.</p>
          <button routerLink="/" class="btn btn-primary mt-3">Return to Marketplace</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .profile-layout {
      margin: 2rem 0;
    }
    .hero-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 3.5rem 2.5rem;
      border-radius: var(--border-radius-lg);
      background: linear-gradient(135deg, var(--primary-glow), var(--surface));
      border: 1px solid var(--surface-border);
    }
    .back-link {
      display: inline-block;
      font-size: 0.9rem;
      font-weight: 500;
    }
    .dot-separator {
      margin: 0 0.5rem;
      color: var(--text-muted);
    }
    .border-bottom {
      border-bottom: 1px solid var(--surface-border);
      padding-bottom: 0.75rem;
      margin-bottom: 1rem;
      font-size: 1.35rem;
    }
    .menu-list {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }
    .menu-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-bottom: 1rem;
      border-bottom: 1px dashed var(--surface-border);
    }
    .menu-item:last-child {
      border-bottom: none;
      padding-bottom: 0;
    }
    .menu-details h4 {
      margin-bottom: 0.25rem;
      font-size: 1.1rem;
      font-weight: 600;
    }
    .price-highlight {
      font-weight: 700;
      color: var(--primary);
      font-size: 1.15rem;
      font-family: var(--font-family-title);
    }
    .staff-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .staff-card {
      display: flex;
      align-items: center;
      gap: 1.25rem;
      padding: 1rem 1.5rem;
      border-radius: var(--border-radius-md);
      background: var(--bg-secondary);
    }
    .staff-avatar {
      width: 45px;
      height: 45px;
      border-radius: 50%;
      background: var(--primary-glow);
      color: var(--primary);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.3rem;
      border: 1px solid var(--primary-glow);
    }
    .staff-details h4 {
      margin-bottom: 0.1rem;
      font-size: 1.05rem;
      font-weight: 600;
    }
    .staff-email {
      font-size: 0.85rem;
      color: var(--text-muted);
    }
    @media (max-width: 768px) {
      .hero-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 1.5rem;
        padding: 2rem 1.5rem;
      }
    }
  `]
})
export class TenantProfileComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private http = inject(HttpClient);
  private tenantContext = inject(TenantContextService);

  readonly tenant = signal<TenantDetail | null>(null);
  readonly services = signal<Service[]>([]);
  readonly staffList = signal<Staff[]>([]);
  readonly loading = signal(false);

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('tenantSlug');
    if (slug) {
      this.loadTenantProfile(slug);
    }
  }

  loadTenantProfile(slug: string): void {
    this.loading.set(true);

    this.http.get<TenantDetail>(`/api/public/tenants/${slug}`)
      .subscribe({
        next: (data) => {
          this.tenant.set(data);
          this.loadSubResources(data.id);
        },
        error: (err) => {
          console.warn('Failed to load public tenant profile, using mock fallback', err);
          // Set mock data based on slug name
          const mockTenant: TenantDetail = {
            id: 't1',
            name: slug.split('-').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' '),
            slug: slug,
            timeZoneId: 'Asia/Tashkent',
            status: 'Active'
          };
          this.tenant.set(mockTenant);
          this.loadSubResources(mockTenant.id);
        }
      });
  }

  loadSubResources(tenantId: string): void {
    // Services
    this.http.get<Service[]>(`/api/public/tenants/${tenantId}/services`)
      .subscribe({
        next: (data) => this.services.set(data),
        error: () => {
          this.services.set([
            { id: 's1', name: 'Teeth Cleaning & Whitening', durationMinutes: 45, price: 150000, currency: 'UZS' },
            { id: 's2', name: 'Root Canal Treatment', durationMinutes: 60, price: 300000, currency: 'UZS' }
          ]);
        }
      });

    // Staffing
    this.http.get<Staff[]>(`/api/public/tenants/${tenantId}/staff`)
      .subscribe({
        next: (data) => {
          this.staffList.set(data);
          this.loading.set(false);
        },
        error: () => {
          this.staffList.set([
            { id: 'st1', displayName: 'Dr. John Doe', email: 'john@smile.com' },
            { id: 'st2', displayName: 'Dr. Jane Smith', email: 'jane@smile.com' }
          ]);
          this.loading.set(false);
        }
      });
  }

  selectTenant(t: TenantDetail): void {
    this.tenantContext.setTenant({
      tenantId: t.id,
      tenantSlug: t.slug,
      timeZoneId: t.timeZoneId
    });
  }
}
