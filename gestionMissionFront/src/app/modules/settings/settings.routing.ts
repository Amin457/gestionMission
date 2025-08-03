import { Routes } from '@angular/router';

export const settingsRoutes: Routes = [
  { 
    path: 'users', 
    loadChildren: () => import('./users/users.routing').then(r => r.usersRoutes)
  },
  {
    path: 'notifications/history',
    loadComponent: () => import('./notification-history/notification-history.component').then(c => c.NotificationHistoryComponent)
  },
  {
    path: 'notifications',
    loadComponent: () => import('./notification-center/notification-center.component').then(c => c.NotificationCenterComponent)
  },
];