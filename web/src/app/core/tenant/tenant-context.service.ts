import { Injectable, signal, computed } from '@angular/core';

export interface TenantInfo {
  tenantId: string;
  tenantSlug: string;
  timeZoneId: string;
}

@Injectable({
  providedIn: 'root'
})
export class TenantContextService {
  // Core signals for tenant state
  readonly tenantId = signal<string | null>(this.getStoredTenantId());
  readonly tenantSlug = signal<string | null>(this.getStoredTenantSlug());
  readonly timeZoneId = signal<string | null>(this.getStoredTimeZoneId());
  readonly isPlatformScope = signal<boolean>(false);

  // Computeds
  readonly hasSelectedTenant = computed(() => this.tenantId() !== null);

  setTenant(info: TenantInfo | null): void {
    if (info) {
      this.tenantId.set(info.tenantId);
      this.tenantSlug.set(info.tenantSlug);
      this.timeZoneId.set(info.timeZoneId);
      this.isPlatformScope.set(false);
      
      // Persist to local storage
      localStorage.setItem('rf_tenant_id', info.tenantId);
      localStorage.setItem('rf_tenant_slug', info.tenantSlug);
      localStorage.setItem('rf_tenant_timezone', info.timeZoneId);
    } else {
      this.tenantId.set(null);
      this.tenantSlug.set(null);
      this.timeZoneId.set(null);
      
      localStorage.removeItem('rf_tenant_id');
      localStorage.removeItem('rf_tenant_slug');
      localStorage.removeItem('rf_tenant_timezone');
    }
  }

  setPlatformScope(enabled: boolean): void {
    this.isPlatformScope.set(enabled);
    if (enabled) {
      // Clear specific tenant selection context in platform scope
      this.tenantId.set(null);
      this.tenantSlug.set(null);
      this.timeZoneId.set(null);
    }
  }

  private getStoredTenantId(): string | null {
    return localStorage.getItem('rf_tenant_id');
  }

  private getStoredTenantSlug(): string | null {
    return localStorage.getItem('rf_tenant_slug');
  }

  private getStoredTimeZoneId(): string | null {
    return localStorage.getItem('rf_tenant_timezone');
  }
}
