import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { loginGuard } from './core/guards/login.guard';

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
        canActivate: [authGuard, adminGuard],
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
        canActivate: [authGuard],
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
        canActivate: [authGuard, adminGuard],
      },
      {
        path: 'release-management',
        loadComponent: () =>
          import('./pages/release-management/release-management.component').then(
            (m) => m.ReleaseManagementComponent,
          ),
        canActivate: [authGuard, adminGuard],
      },
      {
        path: 'release-management/new',
        loadComponent: () =>
          import('./pages/release-management/release-form/release-form.component').then(
            (m) => m.ReleaseFormComponent,
          ),
        canActivate: [authGuard, adminGuard],
      },
      {
        path: 'release-management/edit/:id',
        loadComponent: () =>
          import('./pages/release-management/release-form/release-form.component').then(
            (m) => m.ReleaseFormComponent,
          ),
        canActivate: [authGuard, adminGuard],
      },
      {
        path: 'release-management/:id',
        loadComponent: () =>
          import('./pages/release-management/release-details/release-details.component').then(
            (m) => m.ReleaseDetailsComponent,
          ),
        canActivate: [authGuard, adminGuard],
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
        path: 'settings',
        loadComponent: () =>
          import('./pages/settings/settings.component').then(
            (m) => m.SettingsComponent,
          ),
        canActivate: [authGuard],
      },
    ],
  },
];
