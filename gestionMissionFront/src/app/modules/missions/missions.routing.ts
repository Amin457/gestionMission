import { Routes, RouterModule } from '@angular/router';

export const missionsRoutes: Routes = [
  { 
      path: 'list', 
      loadComponent: () => import('./mission-list/mission-list.component').then(m => m.MissionListComponent)
   },
   { path: 'calendar', 
      loadComponent: () => import('./mission-calendar/mission-calendar.component').then(m => m.MissionCalendarComponent)
    },
   { 
      path: 'tasks/:missionId', 
      loadComponent: () => import('./mission-tasks/mission-tasks.component').then(m => m.MissionTasksComponent)
    },
   { 
      path: 'costs', 
      loadComponent: () => import('./mission-costs/mission-costs.component').then(m => m.MissionCostsComponent)
    },
   { 
      path: 'sheet/:missionId', 
      loadComponent: () => import('./mission-sheet/mission-sheet.component').then(m => m.MissionSheetComponent)
    },
   { 
      path: 'incidents', 
      loadComponent: () => import('./incidents/incidents.component').then(m => m.IncidentsComponent)
    }
];
