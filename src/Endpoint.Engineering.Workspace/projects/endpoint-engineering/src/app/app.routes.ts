import { Routes } from '@angular/router';
import { MainLayout } from './shell/main-layout';
import { Home } from './pages/home/home';
import { RequestsList } from './pages/requests-list/requests-list';
import { RequestCreate } from './pages/request-create/request-create';
import { RequestView } from './pages/request-view/request-view';

export const routes: Routes = [
  {
    path: '',
    component: MainLayout,
    children: [
      {
        path: '',
        component: Home
      },
      {
        path: 'requests',
        component: RequestsList
      },
      {
        path: 'requests/create',
        component: RequestCreate
      },
      {
        path: 'requests/:id',
        component: RequestView
      }
    ]
  }
];
