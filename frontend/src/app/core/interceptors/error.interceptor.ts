import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

/**
 * Functional HTTP interceptor capturing global API exceptions, managing token expiration,
 * and dispatching user-facing error notifications.
 * @param req Outgoing HTTP request.
 * @param next Next handler in the interceptor chain.
 * @returns Observable HTTP stream emitting response or catching failures.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const notificationService = inject(NotificationService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred. Please try again.';

      if (error.status === 401) {
        // Expired or invalid JWT token
        authService.logout();
        notificationService.showError('Session expired. Please sign in again.', 'Unauthorized');
      } else if (error.status === 403) {
        // Forbidden - Insufficient role permissions
        notificationService.showError('You lack permissions to perform this action.', 'Forbidden');
        router.navigate(['/dashboard']);
      } else if (error.status === 400) {
        // Bad request / Validation problem details
        errorMessage = error.error?.detail || error.error?.message || 'Invalid request parameters.';
        notificationService.showError(errorMessage, 'Validation Error');
      } else if (error.status === 500) {
        // Internal server crash
        errorMessage = error.error?.detail || 'Internal server error encountered.';
        notificationService.showError(errorMessage, 'Server Error');
      } else if (error.status === 0) {
        // Network disconnect or CORS failure
        errorMessage = 'Unable to connect to the API server. Please check your network or server status.';
        notificationService.showError(errorMessage, 'Connection Error');
      }

      return throwError(() => error);
    })
  );
};
