import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { Router } from '@angular/router';

interface LoginResponse {
  token: string;
}

interface AuthConfig {
  allowRegistration: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly tokenKey = 'auth_token';
  private readonly apiBaseUrl = '/api/v1/auth';

  constructor(
    private readonly httpClient: HttpClient,
    private readonly router: Router
  ) {}

  async login(username: string, password: string): Promise<boolean> {
    try {
      const response = await firstValueFrom(
        this.httpClient.post<LoginResponse>(`${this.apiBaseUrl}/login`, { username, password })
      );
      localStorage.setItem(this.tokenKey, response.token);
      return true;
    } catch {
      return false;
    }
  }

  async register(username: string, password: string): Promise<{ success: boolean; error?: string }> {
    try {
      const response = await firstValueFrom(
        this.httpClient.post<LoginResponse>(`${this.apiBaseUrl}/register`, { username, password })
      );
      localStorage.setItem(this.tokenKey, response.token);
      return { success: true };
    } catch (err: unknown) {
      const status = (err as { status?: number })?.status;
      if (status === 409) return { success: false, error: 'Username is already taken.' };
      if (status === 403) return { success: false, error: 'Registration is disabled.' };
      return { success: false, error: 'Registration failed. Please try again.' };
    }
  }

  async getConfig(): Promise<AuthConfig> {
    try {
      return await firstValueFrom(
        this.httpClient.get<AuthConfig>(`${this.apiBaseUrl}/config`)
      );
    } catch {
      return { allowRegistration: false };
    }
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }
}
