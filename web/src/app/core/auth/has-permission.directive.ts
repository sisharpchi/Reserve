import { Directive, Input, TemplateRef, ViewContainerRef, inject, OnInit } from '@angular/core';
import { KeycloakAuthService } from './keycloak-auth.service';
import { HttpClient } from '@angular/common/http';

@Directive({
  selector: '[appHasPermission]',
  standalone: true
})
export class HasPermissionDirective implements OnInit {
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);
  private authService = inject(KeycloakAuthService);
  private http = inject(HttpClient);

  private hasView = false;
  private userPermissions: string[] = [];
  private requiredPermission = '';

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.http.get<any>('/api/auth/me').subscribe({
        next: (data) => {
          this.userPermissions = data?.permissions || [];
          this.updateView();
        },
        error: () => {
          // Fallback permissions based on roles for demonstration
          const roles = this.authService.roles().map(r => r.replace(/[-_]/g, '').toLowerCase());
          if (roles.includes('platformadmin') || roles.includes('systemadmin')) {
            this.userPermissions = ['Platform.Tenants.Manage', 'Reports.View'];
          } else if (roles.includes('tenantadmin')) {
            this.userPermissions = ['Tenant.Services.Manage', 'Tenant.Staff.Manage', 'Tenant.Resources.Manage', 'Bookings.ViewAll'];
          } else if (roles.includes('staff')) {
            this.userPermissions = ['Bookings.CompleteAssigned'];
          }
          this.updateView();
        }
      });
    }
  }

  @Input() set appHasPermission(permission: string) {
    this.requiredPermission = permission;
    this.updateView();
  }

  private updateView() {
    if (!this.requiredPermission) {
      this.renderTemplate(true);
      return;
    }

    const hasPermission = this.userPermissions.includes(this.requiredPermission);
    this.renderTemplate(hasPermission);
  }

  private renderTemplate(condition: boolean) {
    if (condition && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!condition && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}
