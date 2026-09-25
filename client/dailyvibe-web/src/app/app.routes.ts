import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';

const loadAuthPage = () => import('./pages/auth/auth.page').then((m) => m.AuthPage);

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/home/home.page').then((m) => m.HomePage),
  },
  { path: 'login', loadComponent: loadAuthPage, data: { mode: 'login' } },
  { path: 'register', loadComponent: loadAuthPage, data: { mode: 'register' } },
  { path: '**', redirectTo: '' },
];
