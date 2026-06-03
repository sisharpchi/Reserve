import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-access-denied',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="container text-center py-5 animated-fade">
      <div class="denied-card glass-card">
        <div class="denied-icon-container">
          <div class="denied-icon">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-12 h-12">
              <path fill-rule="evenodd" d="M12 1.5a5.25 5.25 0 0 0-5.25 5.25v3a3 3 0 0 0-3 3v6.75a3 3 0 0 0 3 3h10.5a3 3 0 0 0 3-3v-6.75a3 3 0 0 0-3-3v-3c0-2.9-2.35-5.25-5.25-5.25Zm-3.75 8.25v-3a3.75 3.75 0 1 1 7.5 0v3h-7.5Z" clip-rule="evenodd" />
            </svg>
          </div>
        </div>
        <h2>Access Restricted</h2>
        <p class="text-secondary mt-3">
          Your account does not possess the required credentials or role scope to view this section. If you believe this is an error, please contact your administrator.
        </p>
        <div class="actions-group mt-4">
          <a routerLink="/" class="btn btn-primary">Return to Marketplace</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .denied-card {
      max-width: 520px;
      margin: 5rem auto;
      padding: 3rem 2.5rem;
      border: 1px solid var(--danger-glow);
      box-shadow: 0 16px 48px rgba(0, 0, 0, 0.4), 0 0 30px var(--danger-glow);
    }
    .denied-icon-container {
      display: flex;
      justify-content: center;
      margin-bottom: 1.5rem;
    }
    .denied-icon {
      width: 72px;
      height: 72px;
      border-radius: 50%;
      background: var(--danger-glow);
      color: var(--danger);
      display: flex;
      align-items: center;
      justify-content: center;
      border: 1px solid var(--danger-glow);
    }
    .denied-icon svg {
      width: 32px;
      height: 32px;
    }
    .actions-group {
      display: flex;
      justify-content: center;
      gap: 1rem;
    }
    .py-5 { padding-top: 4rem; padding-bottom: 4rem; }
    .text-center { text-align: center; }
    .mt-3 { margin-top: 1rem; }
    .mt-4 { margin-top: 2rem; }
  `]
})
export class AccessDeniedComponent {}
