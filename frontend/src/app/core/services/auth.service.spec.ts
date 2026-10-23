import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest } from '../models/auth.models';

/**
 * Unit test suite for AuthService verifying session management, token persistence, and role signals.
 */
describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let mockRouter: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    localStorage.clear();
    mockRouter = {
      navigate: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: mockRouter }
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should authenticate user and update reactive signals on successful login', () => {
    const mockCredentials: LoginRequest = { email: 'test@example.com', password: 'Password@123' };
    const mockResponse: AuthResponse = {
      id: 1,
      fullName: 'Test User',
      email: 'test@example.com',
      role: 'Manager',
      token: 'fake-jwt-token',
      expiresAtUtc: '2026-12-31T23:59:59Z'
    };

    service.login(mockCredentials).subscribe((res) => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);

    expect(service.currentUser()?.email).toBe('test@example.com');
    expect(service.isAuthenticated()).toBe(true);
    expect(service.isManager()).toBe(true);
    expect(localStorage.getItem('elms_auth_token')).toBe('fake-jwt-token');
  });

  it('should clear stored session, reset signals, and redirect to login on logout', () => {
    localStorage.setItem('elms_auth_token', 'sample-token');
    localStorage.setItem('elms_auth_user', JSON.stringify({ id: 1, fullName: 'Test', email: 'a@b.com', role: 'Employee' }));

    service.logout();

    expect(service.getToken()).toBeNull();
    expect(service.currentUser()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  });
});
