import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { authGuard } from './auth.guard';
import { KeycloakAuthService } from '../auth/keycloak-auth.service';

describe('authGuard', () => {
  let authServiceMock: any;
  let routerMock: any;

  beforeEach(() => {
    authServiceMock = {
      isAuthenticated: jasmine.createSpy('isAuthenticated'),
      login: jasmine.createSpy('login'),
      roles: jasmine.createSpy('roles').and.returnValue([])
    };

    routerMock = {
      navigate: jasmine.createSpy('navigate')
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: KeycloakAuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    });
  });

  it('should navigate to login if not authenticated', () => {
    authServiceMock.isAuthenticated.and.returnValue(false);

    const guardFn = authGuard([]);
    const result = TestBed.runInInjectionContext(() => guardFn({} as any, {} as any));

    expect(result).toBeFalse();
    expect(authServiceMock.login).toHaveBeenCalled();
  });

  it('should allow navigation if authenticated and no roles required', () => {
    authServiceMock.isAuthenticated.and.returnValue(true);

    const guardFn = authGuard([]);
    const result = TestBed.runInInjectionContext(() => guardFn({} as any, {} as any));

    expect(result).toBeTrue();
    expect(authServiceMock.login).not.toHaveBeenCalled();
    expect(routerMock.navigate).not.toHaveBeenCalled();
  });

  it('should navigate to access-denied if authenticated but missing required role', () => {
    authServiceMock.isAuthenticated.and.returnValue(true);
    authServiceMock.roles.and.returnValue(['user-role']);

    const guardFn = authGuard(['admin-role']);
    const result = TestBed.runInInjectionContext(() => guardFn({} as any, {} as any));

    expect(result).toBeFalse();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/access-denied']);
  });

  it('should allow navigation if authenticated and contains required role', () => {
    authServiceMock.isAuthenticated.and.returnValue(true);
    authServiceMock.roles.and.returnValue(['admin-role']);

    const guardFn = authGuard(['admin-role']);
    const result = TestBed.runInInjectionContext(() => guardFn({} as any, {} as any));

    expect(result).toBeTrue();
    expect(routerMock.navigate).not.toHaveBeenCalled();
  });

  it('should match roles case-insensitively and ignore hyphens/underscores', () => {
    authServiceMock.isAuthenticated.and.returnValue(true);
    authServiceMock.roles.and.returnValue(['Tenant-Admin']);

    const guardFn = authGuard(['tenantadmin']);
    const result = TestBed.runInInjectionContext(() => guardFn({} as any, {} as any));

    expect(result).toBeTrue();
    expect(routerMock.navigate).not.toHaveBeenCalled();
  });
});
