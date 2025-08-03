import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { HttpClientModule } from '@angular/common/http';
import { MissionService } from '../../../core/services/mission.service';
import { UserService } from '../../../core/services/User.service';
import { Mission, MissionGet } from '../../../core/models/Mission';
import { User } from '../../../core/models/User';
import { MissionType, MissionStatus } from '../../../core/enums/mission.enums';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { Router } from '@angular/router';

interface CalendarView {
  label: string;
  value: string;
  icon: string;
}

interface DayData {
  date: Date;
  missions: MissionGet[];
  isToday: boolean;
  isWeekend: boolean;
}

interface DriverDayData {
  driver: User;
  days: DayData[];
  totalMissions: number;
}

@Component({
  selector: 'app-mission-calendar',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    HttpClientModule,
    CalendarModule,
    DropdownModule,
    ButtonModule,
    CardModule,
    BadgeModule,
    TooltipModule,
    DialogModule,
    InputTextModule,
    CheckboxModule,
    ToastModule,
    ConfirmDialogModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './mission-calendar.component.html',
  styleUrls: ['./mission-calendar.component.scss'],
  animations: [
    trigger('fadeInOut', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-10px)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ opacity: 0, transform: 'translateY(-10px)' }))
      ])
    ])
  ]
})
export class MissionCalendarComponent implements OnInit, OnDestroy {
  // Expose enums to template
  MissionStatus = MissionStatus;
  MissionType = MissionType;

  // Data
  missions: MissionGet[] = [];
  drivers: User[] = [];
  selectedMission: MissionGet | null = null;
  loading = false;

  // UI State
  showEventDialog = false;
  showQuickAddDialog = false;
  showFilters = false;
  isEditing = false;

  // Calendar Configuration
  currentDate = new Date();
  currentView: 'week' | 'month' = 'week';
  days: Date[] = [];
  driverDays: DriverDayData[] = [];

  // Filters
  selectedStatuses: MissionStatus[] = [];
  selectedTypes: MissionType[] = [];
  searchTerm = '';

  // Quick Add Form
  quickAddForm = {
    service: '',
    receiver: '',
    driverId: null as number | null,
    desiredDate: null as Date | null,
    type: MissionType.Goods,
    status: MissionStatus.Requested
  };

  // View Options
  calendarViews: CalendarView[] = [
    { label: 'Week', value: 'week', icon: 'pi pi-calendar-times' },
    { label: 'Month', value: 'month', icon: 'pi pi-calendar' }
  ];

  statusOptions = Object.values(MissionStatus).filter(status => typeof status === 'number').map(status => ({
    label: MissionStatus[status as MissionStatus],
    value: status as MissionStatus,
    color: this.getStatusColor(status as MissionStatus)
  }));

  typeOptions = Object.values(MissionType).filter(type => typeof type === 'number').map(type => ({
    label: MissionType[type as MissionType],
    value: type as MissionType,
    color: this.getTypeColor(type as MissionType)
  }));

  constructor(
    private missionService: MissionService,
    private userService: UserService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadData();
    this.generateCalendar();
  }

  ngOnDestroy(): void {
    // Cleanup if needed
  }

  loadData(): void {
    this.loading = true;
    
    // Use forkJoin for parallel requests with modern RxJS
    forkJoin({
      missions: this.missionService.getMissionsPaged(1, 1000, {}), // Get all missions using paged endpoint
      users: this.userService.getAllUsers()
    }).pipe(
      finalize(() => this.loading = false)
    ).subscribe({
      next: (data) => {
        this.missions = data.missions.data || []; // Extract data from paged response
        this.drivers = this.getDriversFromUsers(data.users || []);
        this.generateCalendar();
      },
      error: (error) => {
        console.error('Error loading data:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load mission data'
        });
      }
    });
  }

  getDriversFromUsers(users: User[]): User[] {
    return users.filter((user: User) => user.role === 'Driver');
  }

  // Format drivers for dropdown
  getDriversForDropdown(): any[] {
    return this.drivers.map(driver => ({
      label: `${driver.firstName} ${driver.lastName}`,
      value: driver.userId,
      driver: driver
    }));
  }

  generateCalendar(): void {
    this.days = this.getDaysForView();
    this.driverDays = this.drivers.map(driver => ({
      driver,
      days: this.days.map(date => ({
        date,
        missions: this.getMissionsForDriverAndDate(driver.userId, date),
        isToday: this.isToday(date),
        isWeekend: this.isWeekend(date)
      })),
      totalMissions: this.getTotalMissionsForDriver(driver.userId)
    }));
  }

  getDaysForView(): Date[] {
    const days: Date[] = [];
    const startDate = this.getStartOfView();
    const endDate = this.getEndOfView();
    
    const current = new Date(startDate);
    while (current <= endDate) {
      days.push(new Date(current));
      current.setDate(current.getDate() + 1);
    }
    
    return days;
  }

  getStartOfView(): Date {
    const date = new Date(this.currentDate);
    if (this.currentView === 'week') {
      const day = date.getDay();
      const diff = date.getDate() - day + (day === 0 ? -6 : 1); // Adjust for Sunday
      date.setDate(diff);
    } else {
      date.setDate(1);
    }
    return date;
  }

  getEndOfView(): Date {
    const date = new Date(this.currentDate);
    if (this.currentView === 'week') {
      const start = this.getStartOfView();
      date.setTime(start.getTime());
      date.setDate(date.getDate() + 6);
    } else {
      date.setMonth(date.getMonth() + 1);
      date.setDate(0);
    }
    return date;
  }

  getMissionsForDriverAndDate(driverId: number, date: Date): MissionGet[] {
    return this.missions.filter(mission => {
      if (mission.driverId !== driverId) return false;
      if (!this.filterMission(mission)) return false;
      
      const missionDate = new Date(mission.desiredDate);
      return this.isSameDay(missionDate, date);
    });
  }

  getTotalMissionsForDriver(driverId: number): number {
    return this.missions.filter(m => m.driverId === driverId).length;
  }

  filterMission(mission: MissionGet): boolean {
    // Status filter
    if (this.selectedStatuses.length > 0 && !this.selectedStatuses.includes(mission.status)) {
      return false;
    }

    // Type filter
    if (this.selectedTypes.length > 0 && !this.selectedTypes.includes(mission.type)) {
      return false;
    }

    // Search term filter
    if (this.searchTerm) {
      const searchLower = this.searchTerm.toLowerCase();
      return (
        mission.service.toLowerCase().includes(searchLower) ||
        mission.receiver.toLowerCase().includes(searchLower) ||
        this.getDriverName(mission.driverId).toLowerCase().includes(searchLower)
      );
    }

    return true;
  }

  // Navigation
  previousPeriod(): void {
    if (this.currentView === 'week') {
      this.currentDate.setDate(this.currentDate.getDate() - 7);
    } else {
      this.currentDate.setMonth(this.currentDate.getMonth() - 1);
    }
    this.generateCalendar();
  }

  nextPeriod(): void {
    if (this.currentView === 'week') {
      this.currentDate.setDate(this.currentDate.getDate() + 7);
    } else {
      this.currentDate.setMonth(this.currentDate.getMonth() + 1);
    }
    this.generateCalendar();
  }

  goToToday(): void {
    this.currentDate = new Date();
    this.generateCalendar();
  }

  onViewChange(view: string): void {
    this.currentView = view as 'week' | 'month';
    this.generateCalendar();
  }

  // Event Handlers
  onDayClick(date: Date, driver: User): void {
    this.resetQuickAddForm(); // Reset form and editing state
    this.quickAddForm.desiredDate = date;
    this.quickAddForm.driverId = driver.userId;
    this.showQuickAddDialog = true;
  }

  onMissionClick(mission: MissionGet): void {
    this.selectedMission = mission;
    this.showEventDialog = true;
  }

  // Actions
  onFilterChange(): void {
    this.generateCalendar();
  }

  onQuickAddSubmit(): void {
    if (!this.quickAddForm.service || !this.quickAddForm.receiver || !this.quickAddForm.driverId || !this.quickAddForm.desiredDate) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Please fill in all required fields'
      });
      return;
    }

    if (this.isEditing && this.selectedMission) {
      // Update existing mission
      const updatedMission: Mission = {
        missionId: this.selectedMission.missionId,
        service: this.quickAddForm.service,
        receiver: this.quickAddForm.receiver,
        driverId: this.quickAddForm.driverId,
        requesterId: this.selectedMission.requesterId,
        desiredDate: this.quickAddForm.desiredDate,
        type: this.quickAddForm.type,
        status: this.quickAddForm.status,
        systemDate: this.selectedMission.systemDate || new Date()
      };

      this.missionService.updateMission(this.selectedMission.missionId, updatedMission).subscribe({
        next: () => {
          // Update the mission in the local array
          const index = this.missions.findIndex(m => m.missionId === this.selectedMission!.missionId);
          if (index !== -1) {
            this.missions[index] = {
              ...this.missions[index],
              ...updatedMission
            };
          }
          this.generateCalendar();
          this.showQuickAddDialog = false;
          this.resetQuickAddForm();
          this.isEditing = false;
          this.selectedMission = null;
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Mission updated successfully'
          });
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to update mission'
          });
        }
      });
    } else {
      // Create new mission
      const newMission: Mission = {
        missionId: 0,
        service: this.quickAddForm.service,
        receiver: this.quickAddForm.receiver,
        driverId: this.quickAddForm.driverId,
        requesterId: 1,
        desiredDate: this.quickAddForm.desiredDate,
        type: this.quickAddForm.type,
        status: this.quickAddForm.status,
        systemDate: new Date()
      };

      this.missionService.createMission(newMission).subscribe({
        next: (mission) => {
          const missionGet: MissionGet = {
            ...mission,
            requester: 'Unknown',
            driver: 'Unknown',
            tasks: []
          };
          this.missions.push(missionGet);
          this.generateCalendar();
          this.showQuickAddDialog = false;
          this.resetQuickAddForm();
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Mission created successfully'
          });
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to create mission'
          });
        }
      });
    }
  }

  deleteMission(missionId: number): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete this mission?',
      accept: () => {
        this.missionService.deleteMission(missionId).subscribe({
          next: () => {
            this.missions = this.missions.filter(m => m.missionId !== missionId);
            this.generateCalendar();
            this.showEventDialog = false;
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Mission deleted successfully'
            });
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to delete mission'
            });
          }
        });
      }
    });
  }

  // Utility Methods
  getDriverName(driverId: number): string {
    const driver = this.drivers.find(d => d.userId === driverId);
    return driver ? `${driver.firstName} ${driver.lastName}` : 'Unknown Driver';
  }

  getDriverColor(driverId: number): string {
    const colors = [
      '#3b82f6', '#ef4444', '#10b981', '#f59e0b', '#8b5cf6',
      '#06b6d4', '#84cc16', '#f97316', '#ec4899', '#6366f1'
    ];
    return colors[driverId % colors.length];
  }

  getStatusColor(status: MissionStatus): string {
    const colors = {
      [MissionStatus.Requested]: '#f59e0b',
      [MissionStatus.Approved]: '#10b981',
      [MissionStatus.Planned]: '#3b82f6',
      [MissionStatus.InProgress]: '#3b82f6',
      [MissionStatus.Completed]: '#10b981',
      [MissionStatus.Rejected]: '#ef4444'
    };
    return colors[status] || '#6b7280';
  }

  getTypeColor(type: MissionType): string {
    const colors = {
      [MissionType.Goods]: '#3b82f6',
      [MissionType.Financial]: '#10b981',
      [MissionType.Administrative]: '#f59e0b'
    };
    return colors[type] || '#6b7280';
  }

  isToday(date: Date): boolean {
    const today = new Date();
    return this.isSameDay(date, today);
  }

  isWeekend(date: Date): boolean {
    const day = date.getDay();
    return day === 0 || day === 6;
  }

  isSameDay(date1: Date, date2: Date): boolean {
    return date1.getDate() === date2.getDate() &&
           date1.getMonth() === date2.getMonth() &&
           date1.getFullYear() === date2.getFullYear();
  }

  formatDate(date: Date): string {
    return date.toLocaleDateString('en-US', { 
      weekday: 'short', 
      month: 'short', 
      day: 'numeric' 
    });
  }

  formatDayHeader(date: Date): string {
    if (this.currentView === 'week') {
      return date.toLocaleDateString('en-US', { weekday: 'short', day: 'numeric' });
    } else {
      return date.toLocaleDateString('en-US', { day: 'numeric' });
    }
  }

  resetQuickAddForm(): void {
    this.quickAddForm = {
      service: '',
      receiver: '',
      driverId: null,
      desiredDate: null,
      type: MissionType.Goods,
      status: MissionStatus.Requested
    };
    this.isEditing = false;
    this.selectedMission = null;
  }

  toggleStatusFilter(status: MissionStatus): void {
    if (this.selectedStatuses.includes(status)) {
      this.selectedStatuses = this.selectedStatuses.filter(s => s !== status);
    } else {
      this.selectedStatuses.push(status);
    }
    this.onFilterChange();
  }

  toggleTypeFilter(type: MissionType): void {
    if (this.selectedTypes.includes(type)) {
      this.selectedTypes = this.selectedTypes.filter(t => t !== type);
    } else {
      this.selectedTypes.push(type);
    }
    this.onFilterChange();
  }

  clearFilters(): void {
    this.selectedStatuses = [];
    this.selectedTypes = [];
    this.searchTerm = '';
    this.generateCalendar();
  }

  getViewTitle(): string {
    if (this.currentView === 'week') {
      const start = this.getStartOfView();
      const end = this.getEndOfView();
      return `${start.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} - ${end.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}`;
    } else {
      return this.currentDate.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
    }
  }

  // Edit mission functionality
  editMission(mission: MissionGet): void {
    this.selectedMission = mission;
    this.isEditing = true;
    this.quickAddForm = {
      service: mission.service,
      receiver: mission.receiver,
      driverId: mission.driverId,
      desiredDate: new Date(mission.desiredDate),
      type: mission.type,
      status: mission.status
    };
    this.showQuickAddDialog = true;
    this.showEventDialog = false;
  }

  // Navigate to manage tasks
  navigateToTasks(missionId: number): void {
    // Navigate to mission tasks route
    this.router.navigate([`/missions/tasks/${missionId}`]);
  }

  viewMissionSheet(mission: MissionGet): void {
    this.router.navigate(['/missions/sheet', mission.missionId]);
  }
}
