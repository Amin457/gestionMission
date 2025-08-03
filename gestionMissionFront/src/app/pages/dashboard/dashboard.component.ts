import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil, forkJoin, of } from 'rxjs';
import { catchError, finalize, map } from 'rxjs/operators';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { ProgressBarModule } from 'primeng/progressbar';
import { TagModule } from 'primeng/tag';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { BadgeModule } from 'primeng/badge';
import { SkeletonModule } from 'primeng/skeleton';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

// Services
import { MissionService } from '../../core/services/mission.service';
import { VehicleService } from '../../core/services/vehicle.service';
import { UserService } from '../../core/services/User.service';
import { AuthService } from '../../core/services/auth.service';
import { DashboardStatsService, DashboardOverviewResponse, MissionStatsResponse, VehicleStatsResponse, DriverStatsResponse, ChartDataResponse, RecentActivityResponse, RealTimeStatsResponse, MissionFilter } from '../../core/services/dashboard-stats.service';

// Models and Enums
import { MissionGet, Mission } from '../../core/models/Mission';
import { Vehicle } from '../../core/models/Vehicle';
import { User } from '../../core/models/User';
import { MissionStatus, MissionType } from '../../core/enums/mission.enums';
import { DriverStatus } from '../../core/enums/DriverStatus';
import { VehicleType } from '../../core/enums/VehicleType';

// Interfaces
interface DashboardStats {
  totalMissions: number;
  activeMissions: number;
  completedMissions: number;
  totalVehicles: number;
  availableVehicles: number;
  totalDrivers: number;
  activeDrivers: number;
  pendingMissions: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    CardModule,
    ChartModule,
    DropdownModule,
    CalendarModule,
    ButtonModule,
    InputTextModule,
    MultiSelectModule,
    ProgressBarModule,
    TagModule,
    TableModule,
    TooltipModule,
    BadgeModule,
    SkeletonModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data properties
  stats: DashboardStats = {
    totalMissions: 0,
    activeMissions: 0,
    completedMissions: 0,
    totalVehicles: 0,
    availableVehicles: 0,
    totalDrivers: 0,
    activeDrivers: 0,
    pendingMissions: 0
  };

  // API Response data
  overviewStats?: DashboardOverviewResponse;
  missionStats?: MissionStatsResponse;
  vehicleStats?: VehicleStatsResponse;
  driverStats?: DriverStatsResponse;
  recentActivity?: RecentActivityResponse;
  realTimeStats?: RealTimeStatsResponse;

  // Form controls
  filterForm: FormGroup;

  // Loading states
  isLoading = true;
  isLoadingStats = false;

  // Chart data
  missionStatusChart: any;
  missionTypeChart: any;
  vehicleAvailabilityChart: any;
  driverStatusChart: any;

  // Filter options
  statusOptions = [
    { label: 'Requested', value: MissionStatus.Requested },
    { label: 'Approved', value: MissionStatus.Approved },
    { label: 'Planned', value: MissionStatus.Planned },
    { label: 'In Progress', value: MissionStatus.InProgress },
    { label: 'Completed', value: MissionStatus.Completed },
    { label: 'Rejected', value: MissionStatus.Rejected }
  ];

  typeOptions = [
    { label: 'Goods', value: MissionType.Goods },
    { label: 'Financial', value: MissionType.Financial },
    { label: 'Administrative', value: MissionType.Administrative }
  ];

  driverOptions: any[] = [];

  constructor(
    private missionService: MissionService,
    private vehicleService: VehicleService,
    private userService: UserService,
    public authService: AuthService,
    private dashboardStatsService: DashboardStatsService,
    private fb: FormBuilder,
    private messageService: MessageService,
    private router: Router
  ) {
    this.filterForm = this.fb.group({
      status: [[]],
      type: [[]],
      driverId: [null],
      dateRange: [null],
      requester: ['']
    });
  }

  ngOnInit() {
    this.loadDashboardData();
    this.setupFilterListeners();
    this.initializeCharts();
    this.setupRealTimeUpdates();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadDashboardData() {
    this.isLoading = true;
    
    // Load dashboard overview statistics
    this.dashboardStatsService.getOverview().pipe(
      takeUntil(this.destroy$),
      catchError((error) => {
        console.error('Error loading dashboard overview:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load dashboard overview'
        });
        return of(null);
      })
    ).subscribe({
      next: (overview) => {
        if (overview) {
          this.overviewStats = overview;
          this.updateStatsFromAPI(overview);
        }
      }
    });

    // Load mission statistics
    this.dashboardStatsService.getMissionStats().pipe(
      takeUntil(this.destroy$),
      catchError((error) => {
        console.error('Error loading mission stats:', error);
        return of(null);
      })
    ).subscribe({
      next: (missionStats) => {
        if (missionStats) {
          this.missionStats = missionStats;
          this.updateMissionCharts(missionStats);
        }
      }
    });

    // Load vehicle statistics
    this.dashboardStatsService.getVehicleStats().pipe(
      takeUntil(this.destroy$),
      catchError((error) => {
        console.error('Error loading vehicle stats:', error);
        return of(null);
      })
    ).subscribe({
      next: (vehicleStats) => {
        if (vehicleStats) {
          this.vehicleStats = vehicleStats;
          this.updateVehicleCharts(vehicleStats);
        }
      }
    });

    // Load driver statistics
    this.dashboardStatsService.getDriverStats().pipe(
      takeUntil(this.destroy$),
      catchError((error) => {
        console.error('Error loading driver stats:', error);
        return of(null);
      })
    ).subscribe({
      next: (driverStats) => {
        if (driverStats) {
          this.driverStats = driverStats;
          this.updateDriverCharts(driverStats);
        }
      }
    });

    // Load recent activity
    this.dashboardStatsService.getRecentActivity(5).pipe(
      takeUntil(this.destroy$),
      catchError((error) => {
        console.error('Error loading recent activity:', error);
        return of(null);
      })
    ).subscribe({
      next: (activity) => {
        if (activity) {
          this.recentActivity = activity;
        }
      }
    });

    // Load real-time statistics
    this.dashboardStatsService.getRealTimeStats().pipe(
      takeUntil(this.destroy$),
      catchError((error) => {
        console.error('Error loading real-time stats:', error);
        return of(null);
      }),
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: (realtime) => {
        if (realtime) {
          this.realTimeStats = realtime;
        }
      }
    });
  }

  private updateStatsFromAPI(overview: DashboardOverviewResponse) {
    this.stats = {
      totalMissions: overview.missions.total,
      activeMissions: overview.missions.active,
      completedMissions: overview.missions.completed,
      totalVehicles: overview.vehicles.total,
      availableVehicles: overview.vehicles.available,
      totalDrivers: overview.drivers.total,
      activeDrivers: overview.drivers.active,
      pendingMissions: overview.missions.pending
    };
  }

  private updateMissionCharts(missionStats: MissionStatsResponse) {
    // Update mission status chart
    this.missionStatusChart = {
      labels: ['Requested', 'Approved', 'Planned', 'In Progress', 'Completed', 'Rejected'],
      datasets: [{
        data: [
          missionStats.summary.byStatus.requested,
          missionStats.summary.byStatus.approved,
          missionStats.summary.byStatus.planned,
          missionStats.summary.byStatus.inProgress,
          missionStats.summary.byStatus.completed,
          missionStats.summary.byStatus.rejected
        ],
        backgroundColor: ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40']
      }]
    };

    // Update mission type chart
    this.missionTypeChart = {
      labels: ['Goods', 'Financial', 'Administrative'],
      datasets: [{
        data: [
          missionStats.summary.byType.goods,
          missionStats.summary.byType.financial,
          missionStats.summary.byType.administrative
        ],
        backgroundColor: ['#FF6384', '#36A2EB', '#FFCE56']
      }]
    };
  }

  private updateVehicleCharts(vehicleStats: VehicleStatsResponse) {
    this.vehicleAvailabilityChart = {
      labels: ['Available', 'In Use', 'Maintenance'],
      datasets: [{
        data: [
          vehicleStats.summary.available,
          vehicleStats.summary.inUse,
          vehicleStats.summary.maintenance
        ],
        backgroundColor: ['#4BC0C0', '#FF6384', '#FFCE56']
      }]
    };
  }

  private updateDriverCharts(driverStats: DriverStatsResponse) {
    this.driverStatusChart = {
      labels: ['Available', 'In Transit', 'Off Duty', 'On Break'],
      datasets: [{
        data: [
          driverStats.status.byStatus.available,
          driverStats.status.byStatus.inTransit,
          driverStats.status.byStatus.offDuty,
          driverStats.status.byStatus.onBreak
        ],
        backgroundColor: ['#4BC0C0', '#36A2EB', '#FF6384', '#FFCE56']
      }]
    };
  }

  private setupRealTimeUpdates() {
    // Poll for real-time updates every 30 seconds
    const interval = setInterval(() => {
      this.dashboardStatsService.getRealTimeStats().pipe(
        takeUntil(this.destroy$),
        catchError((error) => {
          console.error('Error loading real-time stats:', error);
          return of(null);
        })
      ).subscribe({
        next: (realtime) => {
          if (realtime) {
            this.realTimeStats = realtime;
          }
        }
      });
    }, 30000);

    // Clear interval on component destroy
    this.destroy$.subscribe(() => {
      clearInterval(interval);
    });
  }

  private setupFilterListeners() {
    this.filterForm.valueChanges.pipe(
      takeUntil(this.destroy$)
    ).subscribe(() => {
      // Use dashboard filtered stats API instead of local filtering
      this.loadFilteredStats();
    });
  }

  private loadFilteredStats() {
    const filters = this.filterForm.value;
    if (!filters.status?.length && !filters.type?.length && !filters.driverId && !filters.dateRange?.length) {
      return; // No filters applied
    }

    this.isLoadingStats = true;
    
    const filterParams: MissionFilter = {
      dateRange: filters.dateRange?.length === 2 ? {
        from: filters.dateRange[0].toISOString(),
        to: filters.dateRange[1].toISOString()
      } : undefined,
      filters: {
        statuses: filters.status,
        types: filters.type,
        driverIds: filters.driverId ? [filters.driverId] : undefined,
        requesters: filters.requester ? [filters.requester] : undefined
      }
    };

    this.dashboardStatsService.getFilteredMissionStats('status', filterParams).pipe(
      takeUntil(this.destroy$),
      catchError((error) => {
        console.error('Error loading filtered stats:', error);
        return of(null);
      }),
      finalize(() => this.isLoadingStats = false)
    ).subscribe({
      next: (filteredStats) => {
        if (filteredStats) {
          this.updateFilteredCharts(filteredStats);
        }
      }
    });
  }

  private updateFilteredCharts(filteredStats: any) {
    // Update charts with filtered data
    if (filteredStats.groupedData) {
      // Update mission status chart with filtered data
      this.missionStatusChart = {
        labels: filteredStats.groupedData.map((item: any) => item.group),
        datasets: [{
          data: filteredStats.groupedData.map((item: any) => item.count),
          backgroundColor: ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40']
        }]
      };
    }
  }

  private initializeCharts() {
    // Initialize charts with empty data
    this.missionStatusChart = {
      labels: ['Requested', 'Approved', 'Planned', 'In Progress', 'Completed', 'Rejected'],
      datasets: [{
        data: [0, 0, 0, 0, 0, 0],
        backgroundColor: ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40']
      }]
    };

    this.missionTypeChart = {
      labels: ['Goods', 'Financial', 'Administrative'],
      datasets: [{
        data: [0, 0, 0],
        backgroundColor: ['#FF6384', '#36A2EB', '#FFCE56']
      }]
    };

    this.vehicleAvailabilityChart = {
      labels: ['Available', 'Unavailable'],
      datasets: [{
        data: [0, 0],
        backgroundColor: ['#4BC0C0', '#FF6384']
      }]
    };

    this.driverStatusChart = {
      labels: ['Off Duty', 'On Break', 'On Leave', 'In Transit'],
      datasets: [{
        data: [0, 0, 0, 0],
        backgroundColor: ['#FF6384', '#FFCE56', '#36A2EB', '#4BC0C0']
      }]
    };
  }

  getStatusSeverity(status: MissionStatus): 'success' | 'info' | 'warning' | 'danger' | 'secondary' | 'contrast' {
    switch (status) {
      case MissionStatus.Completed: return 'success';
      case MissionStatus.InProgress: return 'info';
      case MissionStatus.Planned: return 'warning';
      case MissionStatus.Approved: return 'secondary';
      case MissionStatus.Requested: return 'contrast';
      case MissionStatus.Rejected: return 'danger';
      default: return 'secondary';
    }
  }

  getStatusLabel(status: MissionStatus): string {
    return this.statusOptions.find(opt => opt.value === status)?.label || 'Unknown';
  }

  getTypeLabel(type: MissionType): string {
    return this.typeOptions.find(opt => opt.value === type)?.label || 'Unknown';
  }

  clearFilters() {
    this.filterForm.reset();
  }

  refreshData() {
    this.isLoadingStats = true;
    
    // Refresh all dashboard data
    forkJoin({
      overview: this.dashboardStatsService.getOverview().pipe(catchError(() => of(null))),
      missionStats: this.dashboardStatsService.getMissionStats().pipe(catchError(() => of(null))),
      vehicleStats: this.dashboardStatsService.getVehicleStats().pipe(catchError(() => of(null))),
      driverStats: this.dashboardStatsService.getDriverStats().pipe(catchError(() => of(null))),
      recentActivity: this.dashboardStatsService.getRecentActivity(5).pipe(catchError(() => of(null))),
      realTimeStats: this.dashboardStatsService.getRealTimeStats().pipe(catchError(() => of(null)))
    }).pipe(
      takeUntil(this.destroy$),
      finalize(() => this.isLoadingStats = false)
    ).subscribe({
      next: (data) => {
        if (data.overview) {
          this.overviewStats = data.overview;
          this.updateStatsFromAPI(data.overview);
        }
        if (data.missionStats) {
          this.missionStats = data.missionStats;
          this.updateMissionCharts(data.missionStats);
        }
        if (data.vehicleStats) {
          this.vehicleStats = data.vehicleStats;
          this.updateVehicleCharts(data.vehicleStats);
        }
        if (data.driverStats) {
          this.driverStats = data.driverStats;
          this.updateDriverCharts(data.driverStats);
        }
        if (data.recentActivity) {
          this.recentActivity = data.recentActivity;
        }
        if (data.realTimeStats) {
          this.realTimeStats = data.realTimeStats;
        }

        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Dashboard data refreshed successfully'
        });
      },
      error: (error) => {
        console.error('Error refreshing dashboard data:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to refresh dashboard data'
        });
      }
    });
  }

  navigateToMissions() {
    this.router.navigate(['/missions/list']);
  }

  navigateToVehicles() {
    this.router.navigate(['/data/vehicules']);
  }

  navigateToUsers() {
    this.router.navigate(['/settings/users/users-management']);
  }

  getMissionCompletionRate(): number {
    if (this.stats.totalMissions === 0) return 0;
    return Math.round((this.stats.completedMissions / this.stats.totalMissions) * 100);
  }

  getVehicleInUseRate(): number {
    if (this.stats.totalVehicles === 0) return 0;
    return Math.round(((this.stats.totalVehicles - this.stats.availableVehicles) / this.stats.totalVehicles) * 100);
  }

  getDriverWorkingRate(): number {
    if (this.stats.totalDrivers === 0) return 0;
    return Math.round((this.stats.activeDrivers / this.stats.totalDrivers) * 100);
  }

  // Helper methods for activity display
  getActivityTypeClass(activityType: string | number): string {
    if (typeof activityType === 'string') {
      return activityType.toLowerCase();
    }
    // Handle numeric activity types
    switch (activityType) {
      case 0: return 'missioncreated';
      case 1: return 'missioncompleted';
      case 2: return 'vehicleassigned';
      case 3: return 'driverstatuschanged';
      case 4: return 'incidentreported';
      case 5: return 'missioncostadded';
      default: return 'default';
    }
  }

  getActivityIcon(activityType: string | number): string {
    if (typeof activityType === 'string') {
      switch (activityType) {
        case 'MissionCreated': return 'pi-plus';
        case 'MissionCompleted': return 'pi-check';
        case 'VehicleAssigned': return 'pi-car';
        case 'DriverStatusChanged': return 'pi-user';
        case 'IncidentReported': return 'pi-exclamation-triangle';
        case 'MissionCostAdded': return 'pi-dollar';
        default: return 'pi-info-circle';
      }
    }
    // Handle numeric activity types
    switch (activityType) {
      case 0: return 'pi-plus';
      case 1: return 'pi-check';
      case 2: return 'pi-car';
      case 3: return 'pi-user';
      case 4: return 'pi-exclamation-triangle';
      case 5: return 'pi-dollar';
      default: return 'pi-info-circle';
    }
  }
}
