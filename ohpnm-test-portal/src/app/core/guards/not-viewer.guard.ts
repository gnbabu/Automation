// not-viewer.guard.ts
import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '@services';

// Route-level enforcement for pages hidden behind `*ngIf="!isViewer"` in the sidebar
// (Test Data Management) — Viewers are read-only and this page is about editing test
// input data, so unlike the Execution Panel (kept read-only but still visible), it's
// hidden entirely rather than shown in a disabled state. Runs after authGuard, so a
// Viewer navigating here directly is redirected instead of seeing a read-only page.
export const notViewerGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isViewer()) {
    return true;
  }

  router.navigate(['/test-case-execution-panel']);
  return false;
};
