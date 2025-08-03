import { Routes } from '@angular/router';

export const dataRoutes: Routes = [
  {
    path: 'articles',
    loadComponent: () => import('./articles/articles.component').then(m => m.ArticlesComponent)
  },
  {
    path: 'villes',
    loadComponent: () => import('./cities/cities.component').then(m => m.CitiesComponent)
  },
  {
    path: 'sites',
    loadComponent: () => import('./sites/sites.component').then(m => m.SitesComponent)
  },
  {
    path: 'vehicules',
    loadComponent: () => import('./vehicules/vehicles.component').then(m => m.VehiclesComponent)
  },
  {
    path: '',
    redirectTo: 'articles',
    pathMatch: 'full'
  }
];
