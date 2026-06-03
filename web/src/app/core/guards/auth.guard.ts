import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { KeycloakAuthService } from '../auth/keycloak-auth.service';

export const authGuard = (allowedRoles: string[]): CanActivateFn => {
  return () => {
    const authService = inject(KeycloakAuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
      authService.login();
      return false;
    }

    if (allowedRoles.length === 0) {
      return true;
    }

    // Role verification
    const userRoles = authService.roles().map(r => r.replace(/[-_]/g, '').toLowerCase());
    const hasRole = allowedRoles.some(role => 
      userRoles.includes(role.replace(/[-_]/g, '').toLowerCase())
    );

    if (hasRole) {
      return true;
    }

    // Redirect to access-denied
    router.navigate(['/access-denied']);
    return false;
  };
};
