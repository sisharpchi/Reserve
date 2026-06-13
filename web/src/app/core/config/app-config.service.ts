import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export interface KeycloakConfig {
  url: string;
  realm: string;
  clientId: string;
}

export interface AppConfig {
  apiBaseUrl: string;
  keycloak: KeycloakConfig;
}

@Injectable({
  providedIn: 'root'
})
export class AppConfigService {
  private readonly configSignal = signal<AppConfig | null>(null);

  constructor(private http: HttpClient) {}

  get config(): AppConfig | null {
    return this.configSignal();
  }

  loadConfig(): Promise<void> {
    // Angular serves public folder at root, fetch config.json
    return firstValueFrom(this.http.get<AppConfig>('/config.json'))
      .then(config => {
        this.configSignal.set(config);
      })
      .catch(error => {
        console.error('Failed to load runtime configuration, falling back to local defaults', error);
        this.configSignal.set({
          apiBaseUrl: 'https://localhost:7207',
          keycloak: {
            url: 'http://localhost:18080',
            realm: 'reserveflow',
            clientId: 'reserveflow-web'
          }
        });
      });
  }
}
