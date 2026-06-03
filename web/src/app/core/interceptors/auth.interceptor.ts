import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpEvent } from '@angular/common/http';
import { inject } from '@angular/core';
import { KeycloakAuthService } from '../auth/keycloak-auth.service';
import { TenantContextService } from '../tenant/tenant-context.service';
import { API_BASE_URL } from '../config/app-config';
import { from, Observable } from 'rxjs';
import { switchMap } from 'rxjs/operators';

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> => {
  const authService = inject(KeycloakAuthService);
  const tenantContext = inject(TenantContextService);
  const apiBaseUrl = inject(API_BASE_URL);

  // Generate a Correlation ID (UUID v4 approximation)
  const correlationId = crypto.randomUUID();

  // Prepend API base URL if relative path matches API pattern
  let targetUrl = req.url;
  if (targetUrl.startsWith('/api') || targetUrl.startsWith('api')) {
    const cleanUrl = targetUrl.startsWith('/') ? targetUrl : '/' + targetUrl;
    targetUrl = `${apiBaseUrl}${cleanUrl}`;
  }

  // Add common headers (Correlation ID)
  let headers = req.headers
    .set('X-Correlation-Id', correlationId);

  // Add multi-tenancy context headers if available
  const tenantId = tenantContext.tenantId();
  const tenantSlug = tenantContext.tenantSlug();
  const timeZone = tenantContext.timeZoneId();

  if (tenantId) {
    headers = headers.set('X-Tenant-Id', tenantId);
  }
  if (tenantSlug) {
    headers = headers.set('X-Tenant-Slug', tenantSlug);
  }
  if (timeZone) {
    headers = headers.set('X-Tenant-TimeZone', timeZone);
  }

  // Clone request with baseline headers and target URL
  let clonedReq = req.clone({ 
    url: targetUrl,
    headers 
  });

  // If user is authenticated, retrieve token and attach authorization
  if (authService.isAuthenticated()) {
    return from(authService.getToken()).pipe(
      switchMap(token => {
        const authorizedReq = clonedReq.clone({
          headers: clonedReq.headers.set('Authorization', `Bearer ${token}`)
        });
        return next(authorizedReq);
      })
    );
  }

  return next(clonedReq);
};
