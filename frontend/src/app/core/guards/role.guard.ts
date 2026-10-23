import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * Functional route guard restricting route activation to users holding Manager or Admin roles.
 * @returns True if authorized; otherwise returns a redirect UrlTree to /dashboard.
 */
export const roleGuard: CanActivateFn = (): boolean | UrlTree => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isManager()) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
