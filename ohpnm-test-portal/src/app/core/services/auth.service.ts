// auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { HttpService } from './http.service';
import { IUser, LoginRequest } from '@interfaces';
import { jwtDecode } from 'jwt-decode';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private logoutTimer: any;
  constructor(private httpService: HttpService, private router: Router) {}

  login(loginRequest: LoginRequest): Observable<any> {
    return this.httpService
      .post<{ token: string; user: any }>('Authentication/login', loginRequest)
      .pipe(
        tap((response) => {
          localStorage.setItem('token', response.token);
          localStorage.setItem('currentUser', JSON.stringify(response.user));
          this.startAutoLogout(response.token);
        })
      );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    //this.router.navigate(['/login']);
    this.router.navigate(['/login'], { replaceUrl: true });
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  getLoggedInUser(): IUser | null {
    const loggedInUser = localStorage.getItem('currentUser');

    if (!loggedInUser) return null;

    try {
      const user: IUser = JSON.parse(loggedInUser);
      return user;
    } catch (e) {
      console.error('Error parsing currentUser from localStorage', e);
      return null;
    }
  }

  getLoggedInUserId(): number {
    const loggedInUser = localStorage.getItem('currentUser');

    if (!loggedInUser) return 0;

    try {
      const user = JSON.parse(loggedInUser);
      return Number(user.userId);
    } catch (e) {
      console.error('Error parsing currentUser from localStorage', e);
      return 0;
    }
  }

  startAutoLogout(token?: string): void {
    const jwt = token ?? this.getToken();
    if (!jwt) return;

    try {
      const decoded: any = jwtDecode(jwt);
      const exp = decoded.exp * 1000; // Convert to milliseconds
      const timeout = exp - Date.now();

      if (timeout > 0) {
        this.logoutTimer = setTimeout(() => this.logout(), timeout);
      } else {
        this.logout(); // Token already expired
      }
    } catch (e) {
      this.logout(); // Malformed token
    }
  }
}
