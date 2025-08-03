import { Routes } from '@angular/router';

export const VEHICLES_ROUTES: Routes = [
  {
    path: 'reservation',
    loadComponent: () => import('./vehicle-reservation/vehicle-reservation.component')
      .then(m => m.VehicleReservationComponent)
  }
];

export default VEHICLES_ROUTES; 