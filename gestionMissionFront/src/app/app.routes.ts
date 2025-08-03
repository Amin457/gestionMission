import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './layouts/admin-layout/admin-layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { LoginComponent } from './pages/login/login.component';
import { AuthGuard } from './core/guards/auth.guard';
import { NoAuthGuard } from './core/guards/no-auth.guard';

export const routes: Routes = [
    {
      path: 'login',
      component: LoginComponent,
      canActivate: [NoAuthGuard]
    },
    {
      path: '',
      component: AdminLayoutComponent,
      canActivate: [AuthGuard],
      children: [
        { path: 'dashboard', component: DashboardComponent },
        { 
          path: 'settings',
          loadChildren: () => import('./modules/settings/settings.routing').then(r => r.settingsRoutes)
        },
        {
          path: 'data',
          loadChildren: () => import('./modules/data/data.routes').then(r => r.dataRoutes)
        },
        {
          path: 'missions',
          loadChildren: () => import('./modules/missions/missions.routing').then(r => r.missionsRoutes)
        },
        {
          path: 'vehicles',
          loadChildren: () => import('./modules/vehicles/vehicles.routes').then(r => r.VEHICLES_ROUTES)
        },
        {
          path: 'circuits',
          loadChildren: () => import('./modules/circuits/circuits.routing').then(r => r.circuitsRoutes)
        },
        { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
      ]
    }
  ];