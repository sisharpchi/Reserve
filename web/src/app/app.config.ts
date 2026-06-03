import { ApplicationConfig, provideBrowserGlobalErrorListeners, APP_INITIALIZER } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { KeycloakAuthService } from './core/auth/keycloak-auth.service';
import { AppConfigService } from './core/config/app-config.service';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export function initializeApp(configService: AppConfigService, authService: KeycloakAuthService) {
  return () => configService.loadConfig().then(() => authService.init());
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [AppConfigService, KeycloakAuthService],
      multi: true
    }
  ]
};
