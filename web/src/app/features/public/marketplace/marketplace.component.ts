import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { TenantContextService } from '../../../core/tenant/tenant-context.service';
import { KeycloakAuthService } from '../../../core/auth/keycloak-auth.service';

interface Category {
  id: string;
  name: string;
  slug: string;
}

interface Tenant {
  id: string;
  name: string;
  slug: string;
  status: string;
  categoryId?: string;
  timeZoneId?: string;
}

@Component({
  selector: 'app-marketplace',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="container animated-fade">
      <!-- Premium Hero Header -->
      <header class="hero-header glass">
        <div class="hero-content">
          <h1>Reserve<span>Flow</span></h1>
          <p>Book appointments, services, and resources instantly on our unified multi-tenant scheduling platform.</p>
        </div>
        <div class="hero-action">
          @if (authService.isAuthenticated()) {
            <div class="user-badge-wrapper">
              <span class="badge badge-primary">Logged in as {{ authService.username() }}</span>
              <button class="btn btn-secondary btn-sm" (click)="authService.logout()">Logout</button>
            </div>
          } @else {
            <button class="btn btn-primary" (click)="authService.login()">Get Started / Login</button>
          }
        </div>
      </header>

      <!-- Navigation shortcuts for roles -->
      @if (authService.isAuthenticated()) {
        <div class="shortcuts-row">
          @if (authService.isPlatformAdmin()) {
            <a routerLink="/platform" class="shortcut-card glass hover-card">
              <span class="shortcut-icon">🛡️</span>
              <div>
                <h3>Platform Portal</h3>
                <p>Manage workspaces, categories, and audit logs.</p>
              </div>
            </a>
          }
          @if (authService.isTenantAdmin()) {
            <a routerLink="/admin" class="shortcut-card glass hover-card" (click)="selectFirstTenant()">
              <span class="shortcut-icon">🏢</span>
              <div>
                <h3>Tenant Admin Dashboard</h3>
                <p>Manage bookings, services, resources, and staff.</p>
              </div>
            </a>
          }
          @if (authService.isStaff()) {
            <a routerLink="/staff" class="shortcut-card glass hover-card">
              <span class="shortcut-icon">📅</span>
              <div>
                <h3>Staff Agenda</h3>
                <p>View your scheduled bookings and update statuses.</p>
              </div>
            </a>
          }
        </div>
      }

      <!-- Marketplace Listings -->
      <main class="listings-section">
        <div class="section-header">
          <h2>Browse Categories</h2>
          <p class="subtitle">Select a business area to filter providers</p>
        </div>

        <!-- Categories List -->
        <div class="categories-row">
          <button 
            class="category-btn" 
            [class.active]="selectedCategoryId() === null"
            (click)="filterCategory(null)">
            All Categories
          </button>
          @for (cat of categories(); track cat.id) {
            <button 
              class="category-btn"
              [class.active]="selectedCategoryId() === cat.id"
              (click)="filterCategory(cat.id)">
              {{ cat.name }}
            </button>
          }
        </div>

        <!-- Providers / Tenants Grid -->
        <div class="section-header" style="margin-top: 2.5rem;">
          <h2>Active Booking Providers</h2>
        </div>

        @if (filteredTenants().length === 0) {
          <div class="empty-state-card mt-3">
            <span class="empty-state-icon">🏢</span>
            <h3>No Providers Available</h3>
            <p>There are currently no active providers listed in this category.</p>
            @if (authService.isPlatformAdmin()) {
              <button routerLink="/platform" class="btn btn-primary mt-3">Create First Tenant</button>
            } @else if (!authService.isAuthenticated()) {
              <button (click)="authService.login()" class="btn btn-primary mt-3">Login to Provision Workspace</button>
            }
          </div>
        } @else {
          <div class="grid-3 mt-3">
            @for (tenant of filteredTenants(); track tenant.id) {
              <div class="tenant-card glass-card hover-card">
                <div class="tenant-icon-wrapper">🏢</div>
                <h3>{{ tenant.name }}</h3>
                <div class="badge-wrapper">
                  <span class="badge badge-success">Active</span>
                </div>
                <p class="tenant-tz">Timezone Context: {{ tenant.timeZoneId || 'Asia/Tashkent' }}</p>
                <div class="card-actions">
                  <button 
                    [routerLink]="['/t', tenant.slug]" 
                    class="btn btn-primary btn-sm w-full"
                    (click)="selectTenant(tenant)">
                    View Profile & Book
                  </button>
                </div>
              </div>
            }
          </div>
        }
      </main>
    </div>
  `,
  styles: [`
    .hero-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 3.5rem 2.5rem;
      margin: 2rem 0;
      background: linear-gradient(135deg, var(--primary-glow), var(--surface));
      border-radius: var(--border-radius-lg);
    }
    .hero-content h1 {
      font-size: 3.2rem;
      margin-bottom: 0.5rem;
    }
    .hero-content h1 span {
      color: var(--primary);
    }
    .hero-content p {
      color: var(--text-secondary);
      font-size: 1.15rem;
      max-width: 650px;
      line-height: 1.5;
    }
    .user-badge-wrapper {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 0.75rem;
    }
    .shortcuts-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 1.25rem;
      margin-bottom: 3.5rem;
    }
    .shortcut-card {
      display: flex;
      align-items: center;
      gap: 1.25rem;
      padding: 1.5rem;
      border-radius: var(--border-radius-md);
      transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
      background: linear-gradient(135deg, var(--secondary-glow), var(--surface));
      border: 1px solid var(--surface-border);
    }
    .shortcut-icon {
      font-size: 2.2rem;
    }
    .shortcut-card h3 {
      font-size: 1.15rem;
      margin-bottom: 0.2rem;
    }
    .shortcut-card p {
      color: var(--text-muted);
      font-size: 0.85rem;
      line-height: 1.4;
    }
    .hover-card:hover {
      transform: translateY(-4px);
      border-color: var(--primary) !important;
      box-shadow: var(--shadow-md) !important;
    }
    .listings-section {
      margin-bottom: 4rem;
    }
    .section-header h2 {
      font-size: 1.8rem;
      margin-bottom: 0.25rem;
    }
    .subtitle {
      color: var(--text-muted);
      font-size: 0.95rem;
    }
    .categories-row {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
      margin-top: 1rem;
    }
    .category-btn {
      padding: 0.6rem 1.35rem;
      background: var(--bg-secondary);
      border: 1px solid var(--surface-border);
      border-radius: var(--border-radius-xl);
      color: var(--text-secondary);
      cursor: pointer;
      font-family: var(--font-family-title);
      font-size: 0.9rem;
      font-weight: 600;
      transition: all 0.2s ease;
    }
    .category-btn:hover {
      border-color: var(--text-muted);
      color: var(--text-primary);
    }
    .category-btn.active {
      background: var(--primary);
      color: var(--bg-primary);
      border-color: var(--primary);
    }
    .tenant-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      padding: 2.5rem 2rem;
      background: linear-gradient(180deg, var(--surface), var(--bg-secondary));
    }
    .tenant-icon-wrapper {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      background: var(--primary-glow);
      color: var(--primary);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.8rem;
      margin-bottom: 1.25rem;
      border: 1px solid var(--primary-glow);
    }
    .tenant-card h3 {
      font-size: 1.35rem;
      margin-bottom: 0.5rem;
    }
    .badge-wrapper {
      margin-bottom: 0.75rem;
    }
    .tenant-tz {
      color: var(--text-muted);
      font-size: 0.85rem;
      margin-bottom: 1.75rem;
    }
    .card-actions {
      width: 100%;
    }
    
    @media (max-width: 768px) {
      .hero-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 1.5rem;
        padding: 2rem 1.5rem;
      }
      .user-badge-wrapper {
        align-items: flex-start;
      }
    }
  `]
})
export class MarketplaceComponent implements OnInit {
  private http = inject(HttpClient);
  readonly authService = inject(KeycloakAuthService);
  private tenantContext = inject(TenantContextService);

  readonly categories = signal<Category[]>([]);
  readonly tenants = signal<Tenant[]>([]);
  readonly selectedCategoryId = signal<string | null>(null);

  readonly filteredTenants = computed(() => {
    const categoryId = this.selectedCategoryId();
    const allTenants = this.tenants();
    if (!categoryId) {
      return allTenants.filter(t => t.status === 'Active');
    }
    return allTenants.filter(t => t.status === 'Active' && t.categoryId === categoryId);
  });

  ngOnInit(): void {
    // Clear tenant context on homepage
    this.tenantContext.setTenant(null);
    this.loadData();
  }

  loadData(): void {
    // Load categories from public API
    this.http.get<Category[]>('/api/public/categories')
      .subscribe({
        next: (data) => this.categories.set(data),
        error: (err) => {
          console.warn('Failed to load categories, using fallback mock data', err);
          this.categories.set([
            { id: '1', name: 'Dental Care', slug: 'dental-care' },
            { id: '2', name: 'Beauty Salon', slug: 'beauty-salon' },
            { id: '3', name: 'Coworking Space', slug: 'coworking-space' }
          ]);
        }
      });

    // Load tenants from public API
    this.http.get<Tenant[]>('/api/public/tenants')
      .subscribe({
        next: (data) => this.tenants.set(data),
        error: (err) => {
          console.warn('Failed to load tenants, using fallback mock data', err);
          this.tenants.set([
            { id: 't1', name: 'Smile Dental Clinic', slug: 'smile-dental', status: 'Active', categoryId: '1', timeZoneId: 'Asia/Tashkent' },
            { id: 't2', name: 'Aura Hair & Spa', slug: 'aura-spa', status: 'Active', categoryId: '2', timeZoneId: 'Asia/Tashkent' }
          ]);
        }
      });
  }

  filterCategory(id: string | null): void {
    this.selectedCategoryId.set(id);
  }

  selectTenant(tenant: Tenant): void {
    this.tenantContext.setTenant({
      tenantId: tenant.id,
      tenantSlug: tenant.slug,
      timeZoneId: tenant.timeZoneId || 'UTC'
    });
  }

  selectFirstTenant(): void {
    const active = this.tenants().find(t => t.status === 'Active');
    if (active) {
      this.selectTenant(active);
    }
  }
}
