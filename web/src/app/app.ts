import { Component, signal, inject, effect } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { KeycloakAuthService } from './core/auth/keycloak-auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  readonly authService = inject(KeycloakAuthService);
  
  // Theme Signal (default is light-theme, clean neutral SaaS design)
  readonly isDarkTheme = signal(false);

  constructor() {
    // Effect to reactively synchronize body CSS classes with the theme signal
    effect(() => {
      const isDark = this.isDarkTheme();
      const body = document.body;
      if (isDark) {
        body.classList.remove('light-theme');
        body.classList.add('dark-theme');
      } else {
        body.classList.remove('dark-theme');
        body.classList.add('light-theme');
      }
    });
  }

  toggleTheme(): void {
    this.isDarkTheme.update(val => !val);
  }
}
