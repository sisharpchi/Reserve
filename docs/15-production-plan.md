# ReserveFlow: Production Deployment Plan

This document details the build processes, container configurations, web server integrations, runtime settings, and quality checklists for the ReserveFlow Angular client.

---

## 1. Production Build & Optimization Pipeline
Angular compilation builds static assets via the command:
```bash
npm run build -- --configuration production
```
The production configuration enforces key optimizations:
* **Ahead-of-Time (AOT) Compilation:** Compiles Angular templates to JavaScript during the build phase, reducing runtime overhead.
* **Minification:** Shrinks bundle file sizes.
* **Tree Shaking:** Excludes unused modules from compilation files.
* **Asset Hashing:** Appends unique hashes to compiled file names to prevent browser caching issues.

---

## 2. Containerized Hosting (Dockerfile & Nginx Configuration)
The application compiles static assets and packages them into a lightweight container served by **Nginx**.

### Nginx Configuration File (`nginx.conf`)
```nginx
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;

    # Gzip compression configurations
    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml;
    gzip_min_length 1000;

    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache static assets long-term
    location ~* \.(?:css|js|jpg|jpeg|gif|png|ico|cur|gz|svg|svgz|mp4|ogg|ogv|webm|htc)$ {
        expires 1y;
        access_log off;
        add_header Cache-Control "public";
    }

    # Prevent caching index.html to ensure users get the latest updates
    location = /index.html {
        expires -1;
        add_header Cache-Control "no-store, no-cache, must-revalidate, proxy-revalidate, max-age=0";
    }

    error_page 500 502 503 504 /50x.html;
    location = /50x.html {
        root /usr/share/nginx/html;
    }
}
```

### Dockerfile
```dockerfile
# Stage 1: Build the static assets
FROM node:20-alpine AS build-stage
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build -- --configuration production

# Stage 2: Serve using Nginx
FROM nginx:alpine
COPY --from=build-stage /app/dist/web/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

---

## 3. Dynamic Runtime Configurations
To avoid rebuilding Docker images for each deployment target (e.g., Staging vs. Production), ReserveFlow uses a dynamic runtime configuration strategy:

1. **The Config File (`public/config.json`):**
   ```json
   {
     "apiUrl": "https://api.reserveflow.com",
     "keycloak": {
       "url": "https://auth.reserveflow.com",
       "realm": "reserveflow",
       "clientId": "reserveflow-spa"
     }
   }
   ```
2. **App Bootstrapper Loader:** At startup, Angular fetches the config file using `HttpClient` before initializing components:
   ```typescript
   export function initializeApp(http: HttpClient, configService: AppConfigService) {
     return () => http.get('/public/config.json').toPromise().then(config => {
       configService.setConfig(config);
     });
   }
   ```
3. **Environment Injection:** Container deployments modify `/public/config.json` dynamically via an `entrypoint.sh` script, making rebuilds unnecessary.

---

## 4. Monitoring & Error Tracking
* **Global Error Logging:** The custom Angular `ErrorHandler` maps runtime client errors and forwards them to the backend audit API: `POST /api/audit/logs`.
* **Trace Verification (Correlation IDs):** The API client appends an `X-Correlation-ID` header to requests. If an API request fails, the interceptor displays the Correlation ID, helping developers trace issues through the backend logs.

---

## 5. Deployment Checklists

### Production Release Checklist
* [ ] Nginx configuration prevents caching `index.html`.
* [ ] CORS policies on the API composition root allow requests from the production domain.
* [ ] Dynamic API configuration redirects requests to the production API URL.
* [ ] Keycloak client redirect rules match the production domain exactly.
* [ ] Sentry / Log aggregator monitoring hooks are configured.
* [ ] SSL certificates are installed on the proxy/ingress router.

### Quality Assurance (QA) Checklist
* [ ] Public customer booking wizard completes successfully on mobile devices.
* [ ] Route guards block unauthorized access to platform and tenant admin pages.
* [ ] Tenant suspension blocks public page routing instantly.
* [ ] Keycloak tokens refresh automatically without interrupting user sessions.
* [ ] Forms display inline error validation messages clearly.
* [ ] Empty states display helpful guidance when tables or calendars contain no records.
