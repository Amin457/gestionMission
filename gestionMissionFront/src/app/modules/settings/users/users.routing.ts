import { Routes } from '@angular/router';

export const usersRoutes: Routes = [
  {
    path: 'users-management',
    loadComponent: () => import('./usersManagement/usersManagement.component').then(c => c.UsersManagementComponent)
  },
  {
    path: 'roles-management',
    loadComponent: () => import('./rolesManagement/rolesManagement.component').then(c => c.RolesManagementComponent)
  },
  {
    path: '',
    redirectTo: 'users-management',
    pathMatch: 'full'
  }
];