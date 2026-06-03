import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AppConfigService, AppConfig } from './app-config.service';

describe('AppConfigService', () => {
  let service: AppConfigService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AppConfigService]
    });

    service = TestBed.inject(AppConfigService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
    expect(service.config).toBeNull();
  });

  it('should load config from config.json and set the signal', (done) => {
    const mockConfig: AppConfig = {
      apiBaseUrl: 'https://prod-api.reserveflow.com',
      keycloak: {
        url: 'https://prod-auth.reserveflow.com',
        realm: 'reserveflow-prod',
        clientId: 'web-spa'
      }
    };

    service.loadConfig().then(() => {
      expect(service.config).toEqual(mockConfig);
      done();
    });

    const req = httpMock.expectOne('/config.json');
    expect(req.request.method).toBe('GET');
    req.flush(mockConfig);
  });

  it('should fallback to defaults if config.json fails to load', (done) => {
    service.loadConfig().then(() => {
      expect(service.config).toBeTruthy();
      expect(service.config?.apiBaseUrl).toBe('https://localhost:7207');
      expect(service.config?.keycloak.realm).toBe('reserveflow');
      done();
    });

    const req = httpMock.expectOne('/config.json');
    req.error(new ErrorEvent('Network error'));
  });
});
