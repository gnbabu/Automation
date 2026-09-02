// manager.guard.ts
import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '@services';

// Route-level enforcement for pages hidden behind `*ngIf="isAdmin || isManager"` in the
// sidebar (Dashboard, Test Case Assignment, Release Management + sub-routes) — shared
// between Admin and Manager, unlike the stricter `adminGuard` (Users/Environment
// Management). Runs after authGuard, so a logged-in User/Tester gets redirected rather
// than seeing a broken/empty page if they navigate to one of these routes directly.
export const managerGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.canAccessManagerFeatures()) {
    return true;
  }

  router.navigate(['/test-case-execution-panel']);
  return false;
};
