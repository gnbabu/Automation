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
        path: 'api',
        loadComponent: () =>
          import('./pages/api/api.component').then((m) => m.ApiComponent),
        canActivate: [authGuard],
      },
      {
        path: 'web',
        loadComponent: () =>
          import('./pages/web/web.component').then((m) => m.WebComponent),
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
        path: 'configure-test-data',
        loadComponent: () =>
          import('./pages/test-suite/configure-test-data.component').then(
            (m) => m.ConfigureTestDataComponent
          ),
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
        path: 'test-cases',
        loadComponent: () =>
          import('./pages/test-cases/test-cases.component').then(
            (m) => m.TestCasesComponent
          ),
        canActivate: [authGuard],
      },
      {
        path: 'test-case-manager',
        loadComponent: () =>
          import('./pages/test-case-manager/test-case-manager.component').then(
            (m) => m.TestCaseManagerComponent
          ),
        canActivate: [authGuard],
      },
      {
        path: 'test-case-assignment',
        loadComponent: () =>
          import(
            './pages/test-case-assignment/test-case-assignment.component'
          ).then((m) => m.TestCaseAssignmentComponent),
        canActivate: [authGuard],
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./pages/users/users.component').then((m) => m.UsersComponent),
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
