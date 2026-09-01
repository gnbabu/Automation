// admin.guard.ts
import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '@services';

// Route-level enforcement for pages already hidden behind `*ngIf="isAdmin"` in the
// sidebar (Dashboard, Users, Test Case Assignment, Release/Environment Management).
// Runs after authGuard, so a logged-in non-admin gets redirected rather than seeing a
// broken/empty page if they navigate to one of these routes directly.
export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAdmin()) {
    return true;
  }

  router.navigate(['/test-case-execution-panel']);
  return false;
};
