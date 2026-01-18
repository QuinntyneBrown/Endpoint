import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  },
  {
    path: 'home',
    loadComponent: () => import('./pages/home/home').then(m => m.HomePage)
  },
  {
    path: 'requests',
    loadComponent: () => import('./pages/request-list/request-list').then(m => m.RequestListPage)
  },
  {
    path: 'request/create',
    loadComponent: () => import('./pages/request-create/request-create').then(m => m.RequestCreatePage)
  }
];
