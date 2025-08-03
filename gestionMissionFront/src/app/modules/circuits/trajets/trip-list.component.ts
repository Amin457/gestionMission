import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { HttpClientModule, HttpClient, HttpErrorResponse } from '@angular/common/http';
import { SharedModule } from '../../../shared/shared.module';
import { MissionService } from '../../../core/services/mission.service';
import { MissionGet } from '../../../core/models/Mission';
import { CardModule } from 'primeng/card';
import { PaginatorModule } from 'primeng/paginator';
import { TooltipModule } from 'primeng/tooltip';
import { RippleModule } from 'primeng/ripple';
import { UserService } from '../../../core/services/User.service';
import { User } from '../../../core/models/User';
import { MissionType, MissionStatus } from '../../../core/enums/mission.enums';
import { CircuitService } from '../../../core/services/circuit.service';
import { Circuit } from '../../../core/models/Circuit';
import { CircuitDetailsComponent } from '../circuit-details/circuit-details.component';

@Component({
  selector: 'app-trip-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    CalendarModule,
    FormsModule,
    ReactiveFormsModule,
    ToastModule,
    DialogModule,
    TagModule,
    HttpClientModule,
    SharedModule,
    CardModule,
    PaginatorModule,
    TooltipModule,
    RippleModule,
    CircuitDetailsComponent
  ],
  providers: [
    MessageService,
    MissionService,
    HttpClient,
    UserService,
    CircuitService
  ],
  templateUrl: './trip-list.component.html',
  styleUrls: ['./trip-list.component.scss']
})
export class TripListComponent implements OnInit {
  MissionType = MissionType;
  MissionStatus = MissionStatus;

  typeOptions: { label: string; value: MissionType }[] = Object.entries(MissionType)
    .filter(([key]) => isNaN(Number(key)))
    .map(([key, value]) => ({ label: key, value: value as MissionType }));

  statusOptions: { label: string; value: MissionStatus }[] = Object.entries(MissionStatus)
    .filter(([key]) => isNaN(Number(key)))
    .map(([key, value]) => ({ label: key, value: value as MissionStatus }));

  users: (User & { fullName: string })[] = [];
  missions: MissionGet[] = [];
  totalRecords: number = 0;
  loading: boolean = false;
  displayTripDialog: boolean = false;
  selectedMission: MissionGet | null = null;
  selectedCircuit: Circuit | null = null;
  filterForm: FormGroup;
  isFilterCollapsed: boolean = true;
  currentPage: number = 1;
  pageSize: number = 10;
  rows: number = 10;
  activeStepIndex: number = 0;

  constructor(
    private fb: FormBuilder,
    private messageService: MessageService,
    private missionService: MissionService,
    private userService: UserService,
    private circuitService: CircuitService
  ) {
    this.filterForm = this.createFilterForm();
  }

  ngOnInit() {
    this.loadMissions();
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.userService.getAllUsers().subscribe({
      next: (response: User[]) => {
        this.users = response.map((user) => ({
          ...user,
          fullName: `${user.lastName} ${user.firstName}`,
        }));
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load users' });
        this.loading = false;
      },
    });
  }

  createFilterForm(): FormGroup {
    return this.fb.group({
      type: [null],
      status: [null],
      driverId: [null],
      desiredDateStart: [null],
      desiredDateEnd: [null],
    });
  }

  loadMissions() {
    this.loading = true;
    const filters = this.filterForm.value;
    this.missionService.getMissionsPaged(this.currentPage, this.pageSize, filters)
      .subscribe({
        next: (response) => {
          this.missions = response.data;
          this.totalRecords = response.totalRecords;
          this.loading = false;
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load missions'
          });
          this.loading = false;
        }
      });
  }

  onPageChange(event: any) {
    this.currentPage = event.page + 1;
    this.pageSize = event.rows;
    this.rows = event.rows;
    this.loadMissions();
  }

  viewTrip(mission: MissionGet) {
    this.selectedMission = mission;
    this.loading = true;
    this.circuitService.getCircuitsForMission(mission.missionId).subscribe({
      next: (circuits) => {
        if (circuits && circuits.length > 0) {
          this.selectedCircuit = circuits[0];
          this.displayTripDialog = true;
        } else {
          this.messageService.add({
            severity: 'info',
            summary: 'Info',
            detail: 'No circuit details available for this mission'
          });
        }
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: `Failed to load circuit details: ${error.message}`
        });
        this.loading = false;
      }
    });
  }

  generateTrip(missionId: number): void {
    this.loading = true;
    this.circuitService.generateCircuitsForMission(missionId).subscribe({
      next: (response: { message: string }) => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: response.message || 'Circuits generated successfully' });
        this.loadMissions(); // Reload missions to reflect new circuits
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        const errorMessage = error?.error?.Message || 'Failed to generate circuits';
        this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
        this.loading = false;
      }
    });
  }

  applyFilters() {
    this.currentPage = 1;
    this.loadMissions();
  }

  clearFilters() {
    this.filterForm.reset();
    this.currentPage = 1;
    this.loadMissions();
  }

  getTypeBadgeClass(type: MissionType): string {
    switch (type) {
      case MissionType.Goods:
        return 'bg-blue-100 text-blue-700';
      case MissionType.Financial:
        return 'bg-green-100 text-green-700';
      case MissionType.Administrative:
        return 'bg-purple-100 text-purple-700';
      default:
        return 'bg-gray-100 text-gray-700';
    }
  }

  getStatusBadgeClass(status: MissionStatus): string {
    switch (status) {
      case MissionStatus.Requested:
        return 'bg-yellow-100 text-yellow-700';
      case MissionStatus.Approved:
        return 'bg-green-100 text-green-700';
      case MissionStatus.Planned:
        return 'bg-blue-100 text-blue-700';
      case MissionStatus.InProgress:
        return 'bg-orange-100 text-orange-700';
      case MissionStatus.Completed:
        return 'bg-teal-100 text-teal-700';
      case MissionStatus.Rejected:
        return 'bg-red-100 text-red-700';
      default:
        return 'bg-gray-100 text-gray-700';
    }
  }

  getTypeLabel(type: MissionType): string {
    return MissionType[type] || 'N/A';
  }

  getStatusLabel(status: MissionStatus): string {
    return MissionStatus[status] || 'N/A';
  }

  // New methods for modern design - Filter management
  trackByMissionId(index: number, mission: MissionGet): number {
    return mission.missionId;
  }

  hasActiveFilters(): boolean {
    const formValue = this.filterForm.value;
    return Object.values(formValue).some(value => value !== null && value !== '');
  }

  getActiveFilters(): Array<{key: string, label: string, value: string}> {
    const formValue = this.filterForm.value;
    const activeFilters: Array<{key: string, label: string, value: string}> = [];

    if (formValue.type) {
      const typeOption = this.typeOptions.find(opt => opt.value === formValue.type);
      activeFilters.push({
        key: 'type',
        label: 'Type',
        value: typeOption?.label || formValue.type
      });
    }

    if (formValue.status) {
      const statusOption = this.statusOptions.find(opt => opt.value === formValue.status);
      activeFilters.push({
        key: 'status',
        label: 'Status',
        value: statusOption?.label || formValue.status
      });
    }

    if (formValue.driverId) {
      const driver = this.users.find(user => user.userId === formValue.driverId);
      activeFilters.push({
        key: 'driverId',
        label: 'Driver',
        value: driver?.fullName || 'Unknown'
      });
    }

    if (formValue.desiredDateStart) {
      activeFilters.push({
        key: 'desiredDateStart',
        label: 'Start Date',
        value: new Date(formValue.desiredDateStart).toLocaleDateString()
      });
    }

    if (formValue.desiredDateEnd) {
      activeFilters.push({
        key: 'desiredDateEnd',
        label: 'End Date',
        value: new Date(formValue.desiredDateEnd).toLocaleDateString()
      });
    }

    return activeFilters;
  }

  removeFilter(filterKey: string): void {
    this.filterForm.get(filterKey)?.setValue(null);
    this.applyFilters();
  }

  // Circuit Details Dialog Methods
  trackByRouteId(index: number, route: any): number {
    return route.routeId;
  }

  setActiveStep(index: number): void {
    this.activeStepIndex = index;
  }

  previousStep(): void {
    if (this.activeStepIndex > 0) {
      this.activeStepIndex--;
    }
  }

  nextStep(): void {
    if (this.selectedCircuit && this.activeStepIndex < this.selectedCircuit.routes.length - 1) {
      this.activeStepIndex++;
    }
  }

  getTotalDistance(): number {
    if (!this.selectedCircuit?.routes) return 0;
    return this.selectedCircuit.routes.reduce((total, route) => total + route.distanceKm, 0);
  }
} 