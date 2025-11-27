import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
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
        (m) => m.LoginComponent
      ),
    canActivate: [loginGuard],
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./pages/auth/register/register.component').then(
        (m) => m.RegisterComponent
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
            (m) => m.DashboardComponent
          ),
        canActivate: [authGuard],
      },
      {
        path: 'reports',
        loadComponent: () =>
          import('./pages/reports/reports.component').then(
            (m) => m.ReportsComponent
          ),
        canActivate: [authGuard],
      },
      {
        path: 'test-suite',
        loadComponent: () =>
          import('./pages/test-suite/test-suite.component').then(
            (m) => m.TestSuiteComponent
          ),
        canActivate: [authGuard],
      },
      {
        path: 'test-case-execution-panel',
        loadComponent: () =>
          import(
            './pages/test-case-execution-panel/test-case-execution-panel.component'
          ).then((m) => m.TestCaseExecutionPanelComponent),
        canActivate: [authGuard],
      },
      {
        path: 'test-data-management',
        loadComponent: () =>
          import(
            './pages/test-data-management/test-data-management.component'
          ).then((m) => m.TestDataManagementComponent),
        canActivate: [authGuard],
      },
      {
        path: 'queue-manager',
        loadComponent: () =>
          import('./pages/queue-manager/queue-manager.component').then(
            (m) => m.QueueManagerComponent
          ),
        canActivate: [authGuard],
      },
      {
        path: 'queue-manager/:queueId/details',
        loadComponent: () =>
          import('./pages/queue-manager/queue-detail.component').then(
            (m) => m.QueueDetailsComponent
          ),
        canActivate: [authGuard],
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./pages/users/users.component').then((m) => m.UsersComponent),
        canActivate: [authGuard],
      },
      {
        path: 'test-case-assignment-user',
        loadComponent: () =>
          import(
            './pages/test-case-assignment-user/test-case-assignment-user.component'
          ).then((m) => m.TestCaseAssignmentUserComponent),
        canActivate: [authGuard],
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./pages/settings/settings.component').then(
            (m) => m.SettingsComponent
          ),
        canActivate: [authGuard],
      },
    ],
  },
];
