import { Component, signal, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Component as NgComponent } from '@angular/core';
import { ToastsComponent } from './shared/toasts.component';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, ToastsComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('frontend');
  auth = inject(AuthService);
  private router = inject(Router);

  logout() {
    this.auth.logout();
  }

  isAuthPage(): boolean {
    const url = this.router.url || '';
    return url.startsWith('/login') || url.startsWith('/register');
  }
}
