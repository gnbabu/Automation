// auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { HttpService } from './http.service';
import { IUser, LoginRequest, RegisterRequest } from '@interfaces';
import { jwtDecode } from 'jwt-decode';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private logoutTimer: any;

  // Reactive mirror of localStorage's `currentUser`, so long-lived components that only
  // read it once at construction time (e.g. LeftSidebarComponent, which lives outside
  // <router-outlet> for the whole session) pick up changes made elsewhere (e.g. Settings'
  // "Edit Profile" save) without needing a page reload. `setCurrentUser`/`logout` are the
  // only writers; `getLoggedInUser`/`isAdmin`/etc. still read localStorage directly (kept
  // as-is) since they're called synchronously in places that don't need reactivity.
  private currentUserSubject = new BehaviorSubject<IUser | null>(this.getLoggedInUser());
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private httpService: HttpService, private router: Router) {}

  login(loginRequest: LoginRequest): Observable<any> {
    return this.httpService
      .post<{ token: string; user: any }>('Authentication/login', loginRequest)
      .pipe(
        tap((response) => {
          localStorage.setItem('token', response.token);
          this.setCurrentUser(response.user);
          this.startAutoLogout(response.token);
        })
      );
  }

  // Updates both localStorage (so a page refresh/other tabs still see it) and the
  // reactive currentUser$ stream (so already-open components, notably the sidebar, update
  // immediately). Called on login and whenever Settings saves a profile change.
  setCurrentUser(user: IUser): void {
    localStorage.setItem('currentUser', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  forgotPassword(email: string): Observable<{ message: string }> {
    return this.httpService.post<{ message: string }>(
      'Authentication/forgot-password',
      { email }
    );
  }

  forgotUsername(email: string): Observable<{ message: string }> {
    return this.httpService.post<{ message: string }>(
      'Authentication/forgot-username',
      { email } // ✅ request body
    );
  }

  resetPassword(data: { token: string; newPassword: string }) {
    return this.httpService.post('Authentication/reset-password', data);
  }

  register(registerRequest: RegisterRequest): Observable<any> {
    return this.httpService
      .post<{ result: boolean; message: string }>(
        'Authentication/register',
        registerRequest
      )
      .pipe(
        tap((response) => {
          if (response.result == true)
            this.router.navigate(['/login'], { replaceUrl: true });
        })
      );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
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

  isAdmin(): boolean {
    const loggedInUser = localStorage.getItem('currentUser');

    if (!loggedInUser) return false;

    try {
      const user: IUser = JSON.parse(loggedInUser);
      return user.roleName.toLowerCase() == 'admin';
    } catch (e) {
      console.error('Error parsing currentUser from localStorage', e);
      return false;
    }
  }

  isManager(): boolean {
    const loggedInUser = localStorage.getItem('currentUser');

    if (!loggedInUser) return false;

    try {
      const user: IUser = JSON.parse(loggedInUser);
      return user.roleName.toLowerCase() == 'manager';
    } catch (e) {
      console.error('Error parsing currentUser from localStorage', e);
      return false;
    }
  }

  // Release Management, Dashboard, and Test Case Assignment are shared between Admin
  // and Manager (Managers already receive Release activation/DLLs-ready notifications
  // and are the natural approver role); Users/Environment Management stay Admin-only.
  canAccessManagerFeatures(): boolean {
    return this.isAdmin() || this.isManager();
  }

  isViewer(): boolean {
    const loggedInUser = localStorage.getItem('currentUser');

    if (!loggedInUser) return false;

    try {
      const user: IUser = JSON.parse(loggedInUser);
      return user.roleName.toLowerCase() == 'viewer';
    } catch (e) {
      console.error('Error parsing currentUser from localStorage', e);
      return false;
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
