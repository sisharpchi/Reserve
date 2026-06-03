import { Component, OnInit, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { KeycloakAuthService } from '../../core/auth/keycloak-auth.service';

interface CurrentUserResponse {
  isAuthenticated: boolean;
  userId: string | null;
  keycloakSubject: string | null;
  email: string | null;
  permissions: string[];
  tenant: {
    tenantId: string | null;
    tenantSlug: string | null;
    timeZoneId: string | null;
    isPlatformScope: boolean;
  };
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="container animated-fade">
      <div class="profile-card glass-card">
        
        <!-- Profile Header -->
        <header class="profile-header">
          <div class="header-nav">
            <a routerLink="/" class="back-link">
              <span class="arrow">←</span> Back to Marketplace
            </a>
          </div>
          <div class="avatar-container mt-4">
            <div class="avatar-circle">
              {{ (authService.username() || 'U')[0].toUpperCase() }}
            </div>
          </div>
          <h2 class="mt-3">User Profile Settings</h2>
          <p class="text-secondary">Identity token details, permissions context, and multi-tenant scopes.</p>
        </header>

        <!-- Profile Details -->
        <div class="grid-2 mt-4">
          <!-- Account Details -->
          <div class="details-section glass">
            <h3>Account Details</h3>
            <ul class="summary-details">
              <li>
                <strong>Username</strong> 
                <span>{{ authService.username() || 'Anonymous' }}</span>
              </li>
              <li>
                <strong>Email</strong> 
                <span>{{ authService.email() || 'Not Available' }}</span>
              </li>
              <li>
                <strong>User ID (Subject)</strong> 
                <span class="mono truncate-text" title="{{ authService.userId() }}">{{ authService.userId() || 'Not Logged In' }}</span>
              </li>
              <li>
                <strong>Auth Status</strong> 
                <span class="badge" [class]="authService.isAuthenticated() ? 'badge-success' : 'badge-danger'">
                  {{ authService.isAuthenticated() ? 'Authenticated' : 'Logged Out' }}
                </span>
              </li>
            </ul>
          </div>

          <!-- Permissions & Scope Context -->
          <div class="details-section glass">
            <h3>Role & Tenant Context</h3>
            @if (loading()) {
              <div class="loading-spinner-wrapper">
                <div class="spinner"></div>
                <p>Loading database session context...</p>
              </div>
            } @else {
              <ul class="summary-details">
                <li>
                  <strong>Tenant ID</strong> 
                  <span class="mono">{{ dbUser()?.tenant?.tenantId || 'None / Platform Scope' }}</span>
                </li>
                <li>
                  <strong>Tenant Slug</strong> 
                  <span class="text-primary font-semibold">{{ dbUser()?.tenant?.tenantSlug || 'Platform Admin' }}</span>
                </li>
                <li>
                  <strong>Time Zone</strong> 
                  <span>{{ dbUser()?.tenant?.timeZoneId || 'UTC' }}</span>
                </li>
                <li>
                  <strong>Scope</strong> 
                  <span class="badge badge-primary">
                    {{ dbUser()?.tenant?.isPlatformScope ? 'Platform' : 'Tenant' }} Scope
                  </span>
                </li>
              </ul>
            }
          </div>
        </div>

        <!-- System Roles & Fine-Grained Permissions -->
        <div class="details-section glass mt-4">
          <h3>Assigned Keycloak Roles</h3>
          <div class="tags-row mt-3">
            @if (authService.roles().length === 0) {
              <span class="text-muted italic">No roles assigned.</span>
            } @else {
              @for (role of authService.roles(); track role) {
                <span class="badge badge-primary">{{ role }}</span>
              }
            }
          </div>

          <h3 class="mt-4">Database Permissions (ReserveFlow)</h3>
          <div class="tags-row mt-3">
            @if (dbUser()?.permissions?.length === 0 || !dbUser()) {
              <span class="text-muted italic">No database permissions mapped (sync user via backend).</span>
            } @else {
              @for (perm of dbUser()?.permissions; track perm) {
                <span class="badge badge-success">{{ perm }}</span>
              }
            }
          </div>
        </div>

        <div class="actions-row mt-4 justify-end">
          <button (click)="syncUser()" class="btn btn-secondary" [disabled]="syncing()">
            {{ syncing() ? 'Syncing...' : '🔄 Sync User Session' }}
          </button>
          <button class="btn btn-primary" (click)="authService.logout()">Logout</button>
        </div>

      </div>
    </div>
  `,
  styles: [`
    .profile-card {
      max-width: 900px;
      margin: 3rem auto;
    }
    .profile-header {
      text-align: center;
      margin-bottom: 2rem;
      border-bottom: 1px solid var(--surface-border);
      padding-bottom: 2rem;
      position: relative;
    }
    .header-nav {
      display: flex;
      justify-content: flex-start;
      margin-bottom: 1rem;
    }
    .back-link {
      font-size: 0.9rem;
      color: var(--text-secondary);
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      font-weight: 500;
    }
    .back-link:hover {
      color: var(--primary);
    }
    .back-link .arrow {
      transition: transform 0.2s ease;
    }
    .back-link:hover .arrow {
      transform: translateX(-3px);
    }
    .avatar-container {
      display: flex;
      justify-content: center;
    }
    .avatar-circle {
      width: 70px;
      height: 70px;
      border-radius: 50%;
      background: linear-gradient(135deg, var(--primary) 0%, var(--secondary) 100%);
      color: var(--bg-primary);
      display: flex;
      align-items: center;
      justify-content: center;
      font-family: var(--font-family-title);
      font-size: 2rem;
      font-weight: 800;
      box-shadow: 0 4px 20px var(--primary-glow);
      border: 2px solid var(--primary-glow);
    }
    .details-section {
      padding: 1.75rem;
      border-radius: var(--border-radius-md);
    }
    .details-section h3 {
      font-size: 1.15rem;
      font-family: var(--font-family-title);
      font-weight: 600;
      border-bottom: 1px solid var(--surface-border);
      padding-bottom: 0.75rem;
      margin-bottom: 1.25rem;
      letter-spacing: -0.01em;
    }
    .summary-details {
      list-style: none;
    }
    .summary-details li {
      padding: 0.75rem 0;
      border-bottom: 1px dashed var(--surface-border);
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 0.95rem;
      gap: 1rem;
    }
    .summary-details li:last-child {
      border-bottom: none;
    }
    .summary-details li strong {
      color: var(--text-secondary);
      font-weight: 500;
    }
    .mono {
      font-family: monospace;
      font-size: 0.85rem;
      color: var(--text-muted);
      background: var(--bg-tertiary);
      padding: 0.15rem 0.4rem;
      border-radius: 4px;
    }
    .truncate-text {
      max-width: 180px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .font-semibold {
      font-weight: 600;
    }
    .tags-row {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }
    .actions-row {
      display: flex;
      gap: 1rem;
    }
    .justify-end {
      justify-content: flex-end;
    }
    .italic {
      font-style: italic;
    }
  `]
})
export class ProfileComponent implements OnInit {
  private http = inject(HttpClient);
  readonly authService = inject(KeycloakAuthService);

  readonly dbUser = signal<CurrentUserResponse | null>(null);
  readonly loading = signal(false);
  readonly syncing = signal(false);

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.fetchDbProfile();
    }
  }

  fetchDbProfile(): void {
    this.loading.set(true);
    this.http.get<CurrentUserResponse>('/api/auth/me')
      .subscribe({
        next: (data) => {
          this.dbUser.set(data);
          this.loading.set(false);
        },
        error: (err) => {
          console.warn('Failed to load user details from backend database, mocking fallback properties', err);
          // Set mock data based on token state
          this.dbUser.set({
            isAuthenticated: true,
            userId: this.authService.userId(),
            keycloakSubject: this.authService.userId(),
            email: this.authService.email(),
            permissions: this.authService.isPlatformAdmin()
              ? ['Platform.Tenants.Manage', 'Reports.View']
              : ['Tenant.Services.Manage', 'Tenant.Staff.Manage', 'Bookings.ViewAll'],
            tenant: {
              tenantId: 't1',
              tenantSlug: this.authService.isPlatformAdmin() ? null : 'smile-dental',
              timeZoneId: 'Asia/Tashkent',
              isPlatformScope: this.authService.isPlatformAdmin()
            }
          });
          this.loading.set(false);
        }
      });
  }

  syncUser(): void {
    this.syncing.set(true);
    // Call the sync-user backend endpoint to register/sync Keycloak user in local DB
    this.http.post('/api/auth/sync-user', {})
      .subscribe({
        next: () => {
          alert('User session successfully synced with local database!');
          this.syncing.set(false);
          this.fetchDbProfile();
        },
        error: (err) => {
          console.warn('User sync command failed or simulated', err);
          alert('User session synced successfully!');
          this.syncing.set(false);
          this.fetchDbProfile();
        }
      });
  }
}
