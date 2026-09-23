import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { managerGuard } from './core/guards/manager.guard';
import { notViewerGuard } from './core/guards/not-viewer.guard';
import { loginGuard } from './core/guards/login.guard';
import { unsavedChangesGuard } from './core/guards/unsaved-changes.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/auth/login/login.component').then(
        (m) => m.LoginComponent,
      ),
    canActivate: [loginGuard],
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./pages/auth/register/register.component').then(
        (m) => m.RegisterComponent,
      ),
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./pages/auth/reset-password/reset-password.component').then(
        (m) => m.ResetPasswordComponent,
      ),
  },
  {
    path: '',
    loadComponent: () =>
      import('./pages/layout/layout.component').then((m) => m.LayoutComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent,
          ),
        // Was [authGuard, managerGuard] - blocked Tester/Viewer entirely, even though
        // login.component.ts/login.guard.ts both unconditionally send every role here
        // after login, so Tester/Viewer were silently double-redirected straight to
        // /test-case-execution-panel on every login and never saw a Dashboard at all.
        // Every API this page calls only requires [Authorize] server-side, no role
        // restriction - DashboardComponent itself now shows a personalized view for
        // Tester (see isTester()) instead of gating the whole page.
        canActivate: [authGuard],
      },
      {
        path: 'test-case-execution-panel',
        loadComponent: () =>
          import('./pages/test-case-execution-panel/test-case-execution-panel.component').then(
            (m) => m.TestCaseExecutionPanelComponent,
          ),
        canActivate: [authGuard],
      },
      {
        path: 'test-data-management',
        loadComponent: () =>
          import('./pages/test-data-management/test-data-management.component').then(
            (m) => m.TestDataManagementComponent,
          ),
        canActivate: [authGuard, notViewerGuard],
        canDeactivate: [unsavedChangesGuard],
      },
      {
        path: 'flow-section-management',
        loadComponent: () =>
          import('./pages/flow-section-management/flow-section-management.component').then(
            (m) => m.FlowSectionManagementComponent,
          ),
        canActivate: [authGuard, notViewerGuard],
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./pages/users/users.component').then((m) => m.UsersComponent),
        canActivate: [authGuard, adminGuard],
      },
      {
        path: 'test-case-assignment-user',
        loadComponent: () =>
          import('./pages/test-case-assignment-user/test-case-assignment-user.component').then(
            (m) => m.TestCaseAssignmentUserComponent,
          ),
        canActivate: [authGuard, managerGuard],
      },
      {
        path: 'release-management',
        loadComponent: () =>
          import('./pages/release-management/release-management.component').then(
            (m) => m.ReleaseManagementComponent,
          ),
        canActivate: [authGuard, managerGuard],
      },
      {
        path: 'release-management/new',
        loadComponent: () =>
          import('./pages/release-management/release-form/release-form.component').then(
            (m) => m.ReleaseFormComponent,
          ),
        canActivate: [authGuard, managerGuard],
      },
      {
        path: 'release-management/edit/:id',
        loadComponent: () =>
          import('./pages/release-management/release-form/release-form.component').then(
            (m) => m.ReleaseFormComponent,
          ),
        canActivate: [authGuard, managerGuard],
      },
      {
        path: 'release-management/:id',
        loadComponent: () =>
          import('./pages/release-management/release-details/release-details.component').then(
            (m) => m.ReleaseDetailsComponent,
          ),
        canActivate: [authGuard, managerGuard],
      },
      {
        path: 'environment-management',
        loadComponent: () =>
          import('./pages/environment-management/environment-management.component').then(
            (m) => m.EnvironmentManagementComponent,
          ),
        canActivate: [authGuard, adminGuard],
      },
      {
        path: 'environment-management/new',
        loadComponent: () =>
          import('./pages/environment-management/environment-form/environment-form.component').then(
            (m) => m.EnvironmentFormComponent,
          ),
        canActivate: [authGuard, adminGuard],
      },
      {
        path: 'environment-management/edit/:id',
        loadComponent: () =>
          import('./pages/environment-management/environment-form/environment-form.component').then(
            (m) => m.EnvironmentFormComponent,
          ),
        canActivate: [authGuard, adminGuard],
      },
      {
        path: 'credential-configuration',
        loadComponent: () =>
          import('./pages/credential-configuration/credential-configuration.component').then(
            (m) => m.CredentialConfigurationComponent,
          ),
        canActivate: [authGuard],
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./pages/settings/settings.component').then(
            (m) => m.SettingsComponent,
          ),
        canActivate: [authGuard],
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('./pages/notifications/notifications.component').then(
            (m) => m.NotificationsComponent,
          ),
        canActivate: [authGuard],
      },
    ],
  },
];
