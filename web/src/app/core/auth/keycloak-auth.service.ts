import { Injectable, signal, computed, inject } from '@angular/core';
import Keycloak from 'keycloak-js';
import { AppConfigService } from '../config/app-config.service';

@Injectable({
  providedIn: 'root'
})
export class KeycloakAuthService {
  private keycloak!: Keycloak;
  private readonly configService = inject(AppConfigService);
  
  // Reactive Signals for Auth State
  readonly isInitialized = signal(false);
  readonly isAuthenticated = signal(false);
  readonly username = signal<string | null>(null);
  readonly email = signal<string | null>(null);
  readonly userId = signal<string | null>(null);
  readonly roles = signal<string[]>([]);
  readonly token = signal<string | null>(null);

  // Computeds
  readonly isPlatformAdmin = computed(() => this.hasAnyNormalizedRole(['platform-admin', 'system-admin']));
  readonly isTenantAdmin = computed(() => this.hasAnyNormalizedRole(['tenant-admin']));
  readonly isStaff = computed(() => this.hasAnyNormalizedRole(['staff']));

  init(): Promise<boolean> {
    const keycloakConfig = this.configService.config?.keycloak ?? {
      url: 'http://localhost:18080',
      realm: 'reserveflow',
      clientId: 'reserveflow-web'
    };

    this.keycloak = new Keycloak({
      url: keycloakConfig.url,
      realm: keycloakConfig.realm,
      clientId: keycloakConfig.clientId
    });

    return this.keycloak.init({
      onLoad: 'check-sso',
      silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
      pkceMethod: 'S256'
    }).then(authenticated => {
      this.isAuthenticated.set(authenticated);
      this.isInitialized.set(true);

      if (authenticated) {
        this.token.set(this.keycloak.token || null);
        this.userId.set(this.keycloak.subject || null);
        
        // Load profile and roles
        const profile = this.keycloak.tokenParsed as any;
        if (profile) {
          this.username.set(profile.preferred_username || null);
          this.email.set(profile.email || null);
          
          // Read roles from Keycloak parsed token
          const realmRoles = profile.realm_access?.roles || [];
          const clientRoles = (profile.resource_access as any)?.['reserveflow-web']?.roles || [];
          const combinedRoles = [...realmRoles, ...clientRoles];
          this.roles.set(combinedRoles);
        }
      }
      return authenticated;
    }).catch(error => {
      console.error('Failed to initialize Keycloak', error);
      this.isInitialized.set(true);
      return false;
    });
  }

  login(): Promise<void> {
    return this.keycloak.login();
  }

  register(): Promise<void> {
    return this.keycloak.register();
  }

  logout(redirectUri?: string): Promise<void> {
    return this.keycloak.logout({
      redirectUri: redirectUri || window.location.origin
    });
  }

  getToken(): Promise<string> {
    return new Promise((resolve, reject) => {
      if (!this.isAuthenticated()) {
        reject('User is not authenticated');
        return;
      }
      
      this.keycloak.updateToken(30)
        .then(() => {
          this.token.set(this.keycloak.token || null);
          resolve(this.keycloak.token || '');
        })
        .catch(err => {
          console.error('Failed to refresh token', err);
          this.login();
          reject(err);
        });
    });
  }

  private hasAnyNormalizedRole(allowedRoles: string[]): boolean {
    const userRoles = this.roles().map(r => this.normalizeRole(r));
    return allowedRoles.some(role => userRoles.includes(this.normalizeRole(role)));
  }

  private normalizeRole(role: string): string {
    return role.replace(/[-_]/g, '').toLowerCase();
  }
}
