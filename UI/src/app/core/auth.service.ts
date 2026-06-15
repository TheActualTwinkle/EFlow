import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { catchError, map, of, tap } from 'rxjs';

import type { components } from '../api/contracts';
import { bookingApiBaseUrl } from './environment';

type CurrentUser = components['schemas']['CurrentUserView'];

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly userState = signal<CurrentUser | null>(null);
  private readonly loadingState = signal(false);
  private readonly errorState = signal<string | null>(null);

  readonly user = this.userState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly hasToken = computed(() => this.loadingState() || !!this.userState());
  readonly isAuthenticated = computed(() => !!this.userState());
  readonly primaryRole = computed(() => this.userState()?.roles?.[0] ?? 'Guest');

  constructor(private readonly http: HttpClient) {
    localStorage.removeItem('eflow.token');
    this.loadCurrentUser().subscribe();
  }

  login(username: string, password: string) {
    this.loadingState.set(true);
    this.errorState.set(null);

    return this.http.post(`${bookingApiBaseUrl}/auth/login`, { username, password }, { responseType: 'text' }).pipe(
      map(() => true),
      catchError((error: HttpErrorResponse | Error) => {
        this.errorState.set(this.formatAuthError(error));
        return of(false);
      }),
      tap(() => this.loadingState.set(false)),
    );
  }

  loadCurrentUser() {
    this.loadingState.set(true);

    return this.http.get<CurrentUser>(`${bookingApiBaseUrl}/auth/me`).pipe(
      tap((user) => this.userState.set(user)),
      map(() => true),
      catchError(() => {
        this.logout();
        return of(false);
      }),
      tap(() => this.loadingState.set(false)),
    );
  }

  logoutOnServer() {
    return this.http.post(`${bookingApiBaseUrl}/auth/logout`, {}, { responseType: 'text' }).pipe(
      catchError(() => of(null)),
      tap(() => this.logout()),
      map(() => true),
    );
  }

  logout(): void {
    this.userState.set(null);
    this.loadingState.set(false);
  }

  hasRole(...roles: RoleMatch[]): boolean {
    const normalized = this.userState()?.roles.map((role) => role.toLowerCase()) ?? [];
    return roles.some((role) => normalized.includes(role.toLowerCase()));
  }

  private formatAuthError(error: HttpErrorResponse | Error): string {
    if (error instanceof HttpErrorResponse && error.status === 423) {
      return 'Учётная запись временно заблокирована после нескольких неудачных попыток.';
    }

    if (error instanceof HttpErrorResponse && error.status === 401) {
      return 'Неверный логин или пароль.';
    }

    return 'Не удалось войти. Проверьте доступность API.';
  }
}

type RoleMatch = 'Admin' | 'Teacher' | 'Student';
