import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, CurrentUser, LoginRequest } from '../models/auth.models';

/**
 * Service managing user authentication, token persistence, and reactive identity state via Angular Signals.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly tokenKey = 'elms_auth_token';
  private readonly userKey = 'elms_auth_user';

  /**
   * Reactive signal holding the currently authenticated user profile, initialized from local storage.
   */
  public currentUser = signal<CurrentUser | null>(this.getStoredUser());

  /**
   * Computed signal evaluating to true if a valid user session is currently active.
   */
  public isAuthenticated = computed(() => !!this.currentUser());

  /**
   * Computed signal evaluating to true if the active user holds Manager or Admin privileges.
   */
  public isManager = computed(() => {
    const role = this.currentUser()?.role;
    return role === 'Manager' || role === 'Admin';
  });

  /**
   * Initializes dependencies for HTTP communication and client routing.
   * @param http Angular HTTP client for invoking backend endpoints.
   * @param router Angular router for navigation.
   */
  constructor(private http: HttpClient, private router: Router) {}

  /**
   * Submits employee credentials to the backend login API, persists the JWT, and updates signals.
   * @param credentials The login payload containing email and password.
   * @returns An observable emitting the backend AuthResponse.
   */
  public login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, credentials).pipe(
      tap((res) => {
        localStorage.setItem(this.tokenKey, res.token);
        const user: CurrentUser = {
          id: res.id,
          fullName: res.fullName,
          email: res.email,
          role: res.role
        };
        localStorage.setItem(this.userKey, JSON.stringify(user));
        this.currentUser.set(user);
      })
    );
  }

  /**
   * Clears persisted credentials from browser storage, resets reactive signals, and navigates to login.
   */
  public logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  /**
   * Retrieves the current raw JWT Bearer token from browser local storage.
   * @returns The token string if authenticated; otherwise null.
   */
  public getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  /**
   * Reads and parses cached user session data from local storage.
   * @returns The deserialized CurrentUser object, or null if absent or corrupted.
   */
  private getStoredUser(): CurrentUser | null {
    const raw = localStorage.getItem(this.userKey);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as CurrentUser;
    } catch {
      return null;
    }
  }
}
