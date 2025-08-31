import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

export interface AuthResponse {
  token: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  token = signal<string | null>(localStorage.getItem('token'));

  register(email: string, password: string, fullName?: string, designation?: string) {
    return this.http.post(`${environment.apiBaseUrl}/api/auth/register`, { email, password, fullName, designation });
  }

  login(email: string, password: string) {
    return this.http.post<AuthResponse>(`${environment.apiBaseUrl}/api/auth/login`, { email, password });
  }

  setToken(token: string) {
    localStorage.setItem('token', token);
    this.token.set(token);
  }

  logout() {
    localStorage.removeItem('token');
    this.token.set(null);
    this.router.navigateByUrl('/login');
  }

  isAuthenticated() {
    return !!this.token();
  }

  getUserId(): string | null {
    const t = this.token();
    if (!t) return null;
    try {
      const payload = JSON.parse(atob(t.split('.')[1]));
      return payload?.sub ?? null;
    } catch {
      return null;
    }
  }
}


