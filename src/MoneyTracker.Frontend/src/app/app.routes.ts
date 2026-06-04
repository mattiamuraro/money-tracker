import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login-page.component').then((m) => m.LoginPageComponent),
  },
  {
    path: '',
    loadComponent: () =>
      import('./features/home/home-page.component').then((m) => m.HomePageComponent),
    canActivate: [authGuard],
  },
  {
    path: 'payments',
    loadComponent: () =>
      import('./features/payments/payments-page.component').then((m) => m.PaymentsPageComponent),
    canActivate: [authGuard],
  },
  {
    path: 'forecasts',
    loadComponent: () =>
      import('./features/forecasts/forecasts-page.component').then((m) => m.ForecastsPageComponent),
    canActivate: [authGuard],
  },
  {
    path: 'admin/categories',
    loadComponent: () =>
      import('./features/categories/admin-categories-page.component').then(
        (m) => m.AdminCategoriesPageComponent
      ),
    canActivate: [authGuard],
  },
  { path: '**', redirectTo: '' },
];
