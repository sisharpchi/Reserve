import { InjectionToken, inject } from '@angular/core';
import { AppConfigService } from './app-config.service';

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => {
    const configService = inject(AppConfigService);
    return configService.config?.apiBaseUrl ?? 'https://localhost:7207';
  }
});

