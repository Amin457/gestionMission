import { Routes } from '@angular/router';

export const circuitsRoutes: Routes = [
    {
        path: 'trajets',
        loadComponent: () => import('./trajets/trip-list.component').then(m => m.TripListComponent)
    }
]; 