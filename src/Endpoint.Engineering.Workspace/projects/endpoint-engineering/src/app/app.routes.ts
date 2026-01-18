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
  },
  {
    path: 'request/:id',
    loadComponent: () => import('./pages/request-detail/request-detail').then(m => m.RequestDetailPage)
  },
  {
    path: 'request/:id/edit',
    loadComponent: () => import('./pages/request-create/request-create').then(m => m.RequestCreatePage)
  },
  {
    path: 'settings',
    loadComponent: () => import('./pages/settings/settings').then(m => m.SettingsPage)
  },
  // Placeholder routes for rail navigation
  {
    path: 'explorer',
    redirectTo: 'home'
  },
  {
    path: 'search',
    redirectTo: 'requests'
  },
  // Wildcard route
  {
    path: '**',
    redirectTo: 'home'
  }
];
