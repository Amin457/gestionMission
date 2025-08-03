import { Injectable } from '@angular/core';
import { Observable, forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { MissionService } from './mission.service';
import { VehicleService } from './vehicle.service';
import { UserService } from './User.service';
import { MissionGet } from '../models/Mission';
import { Vehicle } from '../models/Vehicle';
import { User } from '../models/User';
import { MissionStatus, MissionType } from '../enums/mission.enums';
import { DriverStatus } from '../enums/DriverStatus';

export interface DashboardAnalytics {
  missionTrends: {
    daily: number[];
    weekly: number[];
    monthly: number[];
  };
  topDrivers: Array<{
    driver: User;
    missionsCompleted: number;
    averageRating: number;
  }>;
  vehicleUtilization: {
    mostUsed: Vehicle[];
    leastUsed: Vehicle[];
    averageUtilization: number;
  };
  missionEfficiency: {
    averageCompletionTime: number;
    onTimeDeliveryRate: number;
    customerSatisfaction: number;
  };
}

export interface DashboardFilters {
  dateRange?: Date[];
  missionStatus?: MissionStatus[];
  missionType?: MissionType[];
  driverId?: number;
  vehicleId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  constructor(
    private missionService: MissionService,
    private vehicleService: VehicleService,
    private userService: UserService
  ) {}

  getDashboardData(filters?: DashboardFilters): Observable<{
    missions: MissionGet[];
    vehicles: Vehicle[];
    drivers: User[];
    analytics: DashboardAnalytics;
  }> {
    return forkJoin({
      missions: this.missionService.getAllMissions().pipe(catchError(() => of([]))),
      vehicles: this.vehicleService.getAllVehicles().pipe(catchError(() => of([]))),
      drivers: this.userService.getAllUsers().pipe(catchError(() => of([])))
    }).pipe(
      map(data => {
        let filteredMissions = this.applyFilters(data.missions, filters);
        
        return {
          missions: filteredMissions,
          vehicles: data.vehicles,
          drivers: data.drivers.filter(user => user.role === 'Driver'),
          analytics: this.calculateAnalytics(filteredMissions, data.vehicles, data.drivers)
        };
      })
    );
  }

  private applyFilters(missions: MissionGet[], filters?: DashboardFilters): MissionGet[] {
    if (!filters) return missions;

    let filteredMissions = [...missions];

    // Date range filter
    if (filters.dateRange?.length === 2) {
      const startDate = filters.dateRange[0];
      const endDate = filters.dateRange[1];
      filteredMissions = filteredMissions.filter(mission => {
        const missionDate = new Date(mission.desiredDate);
        return missionDate >= startDate && missionDate <= endDate;
      });
    }

    // Status filter
    if (filters.missionStatus?.length) {
      filteredMissions = filteredMissions.filter(mission => 
        filters.missionStatus!.includes(mission.status)
      );
    }

    // Type filter
    if (filters.missionType?.length) {
      filteredMissions = filteredMissions.filter(mission => 
        filters.missionType!.includes(mission.type)
      );
    }

    // Driver filter
    if (filters.driverId) {
      filteredMissions = filteredMissions.filter(mission => 
        mission.driverId === filters.driverId
      );
    }

    return filteredMissions;
  }

  private calculateAnalytics(missions: MissionGet[], vehicles: Vehicle[], drivers: User[]): DashboardAnalytics {
    // Calculate mission trends (mock data for now)
    const missionTrends = {
      daily: this.generateTrendData(7),
      weekly: this.generateTrendData(4),
      monthly: this.generateTrendData(12)
    };

    // Calculate top drivers
    const topDrivers = drivers.slice(0, 5).map(driver => ({
      driver,
      missionsCompleted: Math.floor(Math.random() * 50) + 10,
      averageRating: Math.random() * 2 + 3 // 3-5 rating
    })).sort((a, b) => b.missionsCompleted - a.missionsCompleted);

    // Calculate vehicle utilization
    const vehicleUtilization = {
      mostUsed: vehicles.slice(0, 3),
      leastUsed: vehicles.slice(-3),
      averageUtilization: Math.random() * 40 + 60 // 60-100%
    };

    // Calculate mission efficiency
    const missionEfficiency = {
      averageCompletionTime: Math.random() * 2 + 1, // 1-3 hours
      onTimeDeliveryRate: Math.random() * 20 + 80, // 80-100%
      customerSatisfaction: Math.random() * 2 + 3 // 3-5 rating
    };

    return {
      missionTrends,
      topDrivers,
      vehicleUtilization,
      missionEfficiency
    };
  }

  private generateTrendData(count: number): number[] {
    return Array.from({ length: count }, () => Math.floor(Math.random() * 100) + 10);
  }

  getMissionStatusDistribution(missions: MissionGet[]): { status: string; count: number; percentage: number }[] {
    const statusCounts = new Map<string, number>();
    
    missions.forEach(mission => {
      const status = this.getStatusLabel(mission.status);
      statusCounts.set(status, (statusCounts.get(status) || 0) + 1);
    });

    const total = missions.length;
    return Array.from(statusCounts.entries()).map(([status, count]) => ({
      status,
      count,
      percentage: total > 0 ? Math.round((count / total) * 100) : 0
    }));
  }

  getMissionTypeDistribution(missions: MissionGet[]): { type: string; count: number; percentage: number }[] {
    const typeCounts = new Map<string, number>();
    
    missions.forEach(mission => {
      const type = this.getTypeLabel(mission.type);
      typeCounts.set(type, (typeCounts.get(type) || 0) + 1);
    });

    const total = missions.length;
    return Array.from(typeCounts.entries()).map(([type, count]) => ({
      type,
      count,
      percentage: total > 0 ? Math.round((count / total) * 100) : 0
    }));
  }

  getDriverStatusDistribution(drivers: User[]): { status: string; count: number; percentage: number }[] {
    const statusCounts = new Map<string, number>();
    
    drivers.forEach(driver => {
      const status = this.getDriverStatusLabel(driver.currentDriverStatus);
      statusCounts.set(status, (statusCounts.get(status) || 0) + 1);
    });

    const total = drivers.length;
    return Array.from(statusCounts.entries()).map(([status, count]) => ({
      status,
      count,
      percentage: total > 0 ? Math.round((count / total) * 100) : 0
    }));
  }

  private getStatusLabel(status: MissionStatus): string {
    const statusLabels = {
      [MissionStatus.Requested]: 'Requested',
      [MissionStatus.Approved]: 'Approved',
      [MissionStatus.Planned]: 'Planned',
      [MissionStatus.InProgress]: 'In Progress',
      [MissionStatus.Completed]: 'Completed',
      [MissionStatus.Rejected]: 'Rejected'
    };
    return statusLabels[status] || 'Unknown';
  }

  private getTypeLabel(type: MissionType): string {
    const typeLabels = {
      [MissionType.Goods]: 'Goods',
      [MissionType.Financial]: 'Financial',
      [MissionType.Administrative]: 'Administrative'
    };
    return typeLabels[type] || 'Unknown';
  }

  private getDriverStatusLabel(status: DriverStatus): string {
    const statusLabels = {
      [DriverStatus.OffDuty]: 'Off Duty',
      [DriverStatus.OnBreak]: 'On Break',
      [DriverStatus.OnLeave]: 'On Leave',
      [DriverStatus.InTransit]: 'In Transit'
    };
    return statusLabels[status] || 'Unknown';
  }

  getRecentActivity(missions: MissionGet[], limit: number = 10): MissionGet[] {
    return missions
      .sort((a, b) => new Date(b.desiredDate).getTime() - new Date(a.desiredDate).getTime())
      .slice(0, limit);
  }

  getUpcomingMissions(missions: MissionGet[], limit: number = 5): MissionGet[] {
    const now = new Date();
    return missions
      .filter(mission => new Date(mission.desiredDate) > now)
      .sort((a, b) => new Date(a.desiredDate).getTime() - new Date(b.desiredDate).getTime())
      .slice(0, limit);
  }

  getUrgentMissions(missions: MissionGet[]): MissionGet[] {
    const now = new Date();
    const tomorrow = new Date(now.getTime() + 24 * 60 * 60 * 1000);
    
    return missions.filter(mission => {
      const missionDate = new Date(mission.desiredDate);
      return missionDate <= tomorrow && 
             (mission.status === MissionStatus.Requested || mission.status === MissionStatus.Approved);
    });
  }
} 