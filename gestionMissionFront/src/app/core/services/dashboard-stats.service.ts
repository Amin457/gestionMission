import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// Dashboard API Response Interfaces
export interface DashboardOverviewResponse {
  missions: {
    total: number;
    active: number;
    completed: number;
    pending: number;
    rejected: number;
  };
  vehicles: {
    total: number;
    available: number;
    inUse: number;
  };
  drivers: {
    total: number;
    active: number;
    available: number;
    offDuty: number;
  };
  utilization: {
    missionCompletionRate: number;
    vehicleUtilizationRate: number;
    driverUtilizationRate: number;
  };
}

export interface MissionStatsResponse {
  summary: {
    total: number;
    byStatus: {
      requested: number;
      approved: number;
      planned: number;
      inProgress: number;
      completed: number;
      rejected: number;
    };
    byType: {
      goods: number;
      financial: number;
      administrative: number;
    };
  };
  trends: {
    daily: Array<{
      date: string;
      count: number;
      completed: number;
    }>;
    weekly: Array<{
      week: string;
      count: number;
      completed: number;
    }>;
    monthly: Array<{
      month: string;
      count: number;
      completed: number;
    }>;
  };
  performance: {
    averageCompletionTime: number;
    completionRate: number;
    onTimeDeliveryRate: number;
  };
}

export interface VehicleStatsResponse {
  summary: {
    total: number;
    available: number;
    inUse: number;
    maintenance: number;
  };
  byType: {
    commercial: {
      total: number;
      available: number;
      utilization: number;
    };
    passenger: {
      total: number;
      available: number;
      utilization: number;
    };
    truck: {
      total: number;
      available: number;
      utilization: number;
    };
  };
  utilization: {
    averageUtilization: number;
    peakUtilization: number;
    lowUtilization: number;
  };
  capacity: {
    totalCapacity: number;
    usedCapacity: number;
    availableCapacity: number;
  };
}

export interface DriverStatsResponse {
  summary: {
    total: number;
    active: number;
    available: number;
    offDuty: number;
    onBreak: number;
  };
  performance: {
    topPerformers: Array<{
      driverId: number;
      driverName: string;
      missionsCompleted: number;
      averageRating: number;
      totalDistance: number;
    }>;
    averageMetrics: {
      missionsPerDriver: number;
      averageRating: number;
      averageDistance: number;
    };
  };
  status: {
    byStatus: {
      available: number;
      inTransit: number;
      offDuty: number;
      onBreak: number;
    };
    availabilityTrend: Array<{
      date: string;
      available: number;
      total: number;
    }>;
  };
}

export interface ChartDataResponse {
  labels: string[];
  datasets: Array<{
    label: string;
    data: number[];
    backgroundColor?: string[] | string;
    borderColor?: string;
  }>;
}

export interface RecentActivityResponse {
  activities: Array<{
    id: number;
    type: string;
    title: string;
    description: string;
    timestamp: string;
    userId: number;
    userName: string;
    relatedId: number;
  }>;
  summary: {
    newMissions: number;
    completedMissions: number;
    statusChanges: number;
  };
}

export interface RealTimeStatsResponse {
  timestamp: string;
  missions: {
    active: number;
    completedToday: number;
    pending: number;
  };
  vehicles: {
    available: number;
    inUse: number;
  };
  drivers: {
    available: number;
    inTransit: number;
  };
  alerts: Array<{
    type: string;
    message: string;
    timestamp: string;
  }>;
}

export interface FilteredMissionStatsResponse {
  summary: {
    total: number;
    filtered: number;
  };
  groupedData: Array<{
    group: string;
    count: number;
    percentage: number;
  }>;
  trends: Array<{
    period: string;
    count: number;
  }>;
}

export interface MissionFilter {
  dateRange?: {
    from: string;
    to: string;
  };
  filters?: {
    statuses?: number[];
    types?: number[];
    driverIds?: number[];
    requesters?: string[];
  };
}

@Injectable({
  providedIn: 'root'
})
export class DashboardStatsService {
  private readonly baseUrl = `${environment.apiBaseUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  // Get Dashboard Overview Statistics
  getOverview(): Observable<DashboardOverviewResponse> {
    return this.http.get<DashboardOverviewResponse>(`${this.baseUrl}/overview`);
  }

  // Get Mission Statistics
  getMissionStats(filters?: {
    dateFrom?: string;
    dateTo?: string;
    driverId?: number;
    type?: number;
  }): Observable<MissionStatsResponse> {
    let params = new HttpParams();
    
    if (filters) {
      if (filters.dateFrom) params = params.set('dateFrom', filters.dateFrom);
      if (filters.dateTo) params = params.set('dateTo', filters.dateTo);
      if (filters.driverId) params = params.set('driverId', filters.driverId.toString());
      if (filters.type !== undefined) params = params.set('type', filters.type.toString());
    }

    return this.http.get<MissionStatsResponse>(`${this.baseUrl}/missions/stats`, { params });
  }

  // Get Vehicle Statistics
  getVehicleStats(): Observable<VehicleStatsResponse> {
    return this.http.get<VehicleStatsResponse>(`${this.baseUrl}/vehicles/stats`);
  }

  // Get Driver Statistics
  getDriverStats(filters?: {
    dateFrom?: string;
    dateTo?: string;
  }): Observable<DriverStatsResponse> {
    let params = new HttpParams();
    
    if (filters) {
      if (filters.dateFrom) params = params.set('dateFrom', filters.dateFrom);
      if (filters.dateTo) params = params.set('dateTo', filters.dateTo);
    }

    return this.http.get<DriverStatsResponse>(`${this.baseUrl}/drivers/stats`, { params });
  }

  // Get Chart Data
  getChartData(chartType: string, filters?: {
    dateFrom?: string;
    dateTo?: string;
  }): Observable<ChartDataResponse> {
    let params = new HttpParams();
    
    if (filters) {
      if (filters.dateFrom) params = params.set('dateFrom', filters.dateFrom);
      if (filters.dateTo) params = params.set('dateTo', filters.dateTo);
    }

    return this.http.get<ChartDataResponse>(`${this.baseUrl}/charts/${chartType}`, { params });
  }

  // Get Recent Activity
  getRecentActivity(limit: number = 10): Observable<RecentActivityResponse> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<RecentActivityResponse>(`${this.baseUrl}/recent-activity`, { params });
  }

  // Get Available Chart Types
  getAvailableChartTypes(): Observable<{ chartTypes: Array<{
    type: string;
    description: string;
    parameters: string[];
  }> }> {
    return this.http.get<{ chartTypes: Array<{
      type: string;
      description: string;
      parameters: string[];
    }> }>(`${this.baseUrl}/charts`);
  }

  // Get Filtered Mission Statistics
  getFilteredMissionStats(groupBy: string, filter: MissionFilter): Observable<FilteredMissionStatsResponse> {
    const params = new HttpParams().set('groupBy', groupBy);
    return this.http.post<FilteredMissionStatsResponse>(`${this.baseUrl}/missions/stats/filtered`, filter, { params });
  }

  // Get Real-time Statistics
  getRealTimeStats(): Observable<RealTimeStatsResponse> {
    return this.http.get<RealTimeStatsResponse>(`${this.baseUrl}/realtime`);
  }

  // Get Dashboard Health
  getHealth(): Observable<{
    status: string;
    timestamp: string;
    service: string;
    version: string;
  }> {
    return this.http.get<{
      status: string;
      timestamp: string;
      service: string;
      version: string;
    }>(`${this.baseUrl}/health`);
  }
} 