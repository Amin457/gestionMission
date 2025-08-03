import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { VehicleReservationService, VehicleReservationFilters } from '../../../core/services/vehicleReservation.service';
import { UserService } from '../../../core/services/User.service';
import { VehicleService } from '../../../core/services/vehicle.service';
import { VehicleReservation, VehicleReservationStatus, CreateVehicleReservationRequest } from '../../../core/models/VehicleReservation';
import { User } from '../../../core/models/User';
import { Vehicle } from '../../../core/models/Vehicle';
import { PagedResponse } from '../../../core/models/PagedResponse';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';
import { RippleModule } from 'primeng/ripple';
import { DialogModule } from 'primeng/dialog';
import { CalendarModule } from 'primeng/calendar';
import { CheckboxModule } from 'primeng/checkbox';
import { FormsModule } from '@angular/forms';
import { PaginatorModule } from 'primeng/paginator';

@Component({
  selector: 'app-vehicle-reservation',
  templateUrl: './vehicle-reservation.component.html',
  styleUrls: ['./vehicle-reservation.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    ToastModule,
    ConfirmDialogModule,
    TooltipModule,
    DropdownModule,
    InputTextModule,
    TagModule,
    CardModule,
    RippleModule,
    DialogModule,
    CalendarModule,
    CheckboxModule,
    PaginatorModule
  ],
  providers: [ConfirmationService, MessageService, VehicleReservationService, UserService, VehicleService]
})
export class VehicleReservationComponent implements OnInit {
  reservations: VehicleReservation[] = [];
  totalRecords: number = 0;
  loading: boolean = false;
  pageSize: number = 10;
  currentPage: number = 1;
  filterForm: FormGroup;
  createForm: FormGroup;
  VehicleReservationStatus = VehicleReservationStatus;

  // Filter collapse state
  isFilterCollapsed: boolean = true;
  showAdvancedFilters: boolean = false;

  // Data for dropdowns
  users: User[] = [];
  vehicles: Vehicle[] = [];
  loadingUsers: boolean = false;
  loadingVehicles: boolean = false;

  // Dialog states
  showCreateDialog: boolean = false;
  creating: boolean = false;

  statusOptions = [
    { label: 'Requested', value: VehicleReservationStatus.Requested },
    { label: 'Approved', value: VehicleReservationStatus.Approved },
    { label: 'Rejected', value: VehicleReservationStatus.Rejected }
  ];

  driverOptions = [
    { label: 'Yes', value: true },
    { label: 'No', value: false }
  ];

  constructor(
    private fb: FormBuilder,
    private reservationService: VehicleReservationService,
    private userService: UserService,
    private vehicleService: VehicleService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {
    this.filterForm = this.createFilterForm();
    this.createForm = this.createReservationForm();
  }

  ngOnInit(): void {
    this.loadReservations();
    this.loadUsers();
    this.loadVehicles();
  }

  createFilterForm(): FormGroup {
    return this.fb.group({
      requesterId: [null],
      vehicleId: [null],
      status: [null],
      requiresDriver: [null],
      startDateFrom: [null],
      startDateTo: [null],
      endDateFrom: [null],
      endDateTo: [null],
      departure: [null],
      destination: [null]
    });
  }

  createReservationForm(): FormGroup {
    return this.fb.group({
      requesterId: [null, [Validators.required, Validators.min(1)]],
      vehicleId: [null, [Validators.required, Validators.min(1)]],
      requiresDriver: [false],
      departure: ['', [Validators.required, Validators.maxLength(100)]],
      destination: ['', [Validators.required, Validators.maxLength(100)]],
      startDate: [null, [Validators.required]],
      endDate: [null, [Validators.required]],
      status: [VehicleReservationStatus.Requested]
    });
  }

  loadUsers(): void {
    this.loadingUsers = true;
    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.loadingUsers = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load users'
        });
        this.loadingUsers = false;
      }
    });
  }

  loadVehicles(): void {
    this.loadingVehicles = true;
    this.vehicleService.getAllVehicles().subscribe({
      next: (vehicles) => {
        this.vehicles = vehicles;
        this.loadingVehicles = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load vehicles'
        });
        this.loadingVehicles = false;
      }
    });
  }

  loadReservations(): void {
    this.loading = true;
    
    const filterValues = this.filterForm.value;
    const filters: VehicleReservationFilters = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      requesterId: filterValues.requesterId || undefined,
      vehicleId: filterValues.vehicleId || undefined,
      status: filterValues.status || undefined,
      requiresDriver: filterValues.requiresDriver || undefined,
      startDateFrom: filterValues.startDateFrom || undefined,
      startDateTo: filterValues.startDateTo || undefined,
      endDateFrom: filterValues.endDateFrom || undefined,
      endDateTo: filterValues.endDateTo || undefined,
      departure: filterValues.departure || undefined,
      destination: filterValues.destination || undefined
    };

    this.reservationService.getPaginatedVehicleReservations(filters)
      .subscribe({
        next: (response: PagedResponse<VehicleReservation>) => {
          this.reservations = response.data;
          this.totalRecords = response.totalRecords;
          this.loading = false;
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load reservations'
          });
          this.loading = false;
        }
      });
  }

  onPageChange(event: any): void {
    this.currentPage = Math.floor(event.first / event.rows) + 1;
    this.pageSize = event.rows;
    this.loadReservations();
  }

  // Filter methods
  toggleFilterCollapse(): void {
    this.isFilterCollapsed = !this.isFilterCollapsed;
  }

  toggleAdvancedFilters(): void {
    this.showAdvancedFilters = !this.showAdvancedFilters;
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.loadReservations();
  }

  resetFilter(): void {
    this.filterForm.reset();
    this.currentPage = 1;
    this.loadReservations();
  }

  get filterCount(): number {
    const values = this.filterForm.value;
    let count = 0;
    if (values.requesterId) count++;
    if (values.vehicleId) count++;
    if (values.status !== null) count++;
    if (values.requiresDriver !== null) count++;
    if (values.startDateFrom) count++;
    if (values.startDateTo) count++;
    if (values.endDateFrom) count++;
    if (values.endDateTo) count++;
    if (values.departure) count++;
    if (values.destination) count++;
    return count;
  }

  get hasActiveFilters(): boolean {
    return this.filterCount > 0;
  }

  getStatusLabel(status: VehicleReservationStatus): string {
    switch (status) {
      case VehicleReservationStatus.Requested:
        return 'Requested';
      case VehicleReservationStatus.Approved:
        return 'Approved';
      case VehicleReservationStatus.Rejected:
        return 'Rejected';
      default:
        return 'Unknown';
    }
  }

  getStatusSeverity(status: VehicleReservationStatus): 'success' | 'secondary' | 'info' | 'warning' | 'danger' | 'contrast' {
    switch (status) {
      case VehicleReservationStatus.Requested:
        return 'warning';
      case VehicleReservationStatus.Approved:
        return 'success';
      case VehicleReservationStatus.Rejected:
        return 'danger';
      default:
        return 'secondary';
    }
  }

  getRequesterName(requesterId: number): string {
    const user = this.users.find(u => u.userId === requesterId);
    return user ? `${user.firstName} ${user.lastName}` : `User ${requesterId}`;
  }

  getVehicleInfo(vehicleId: number): string {
    const vehicle = this.vehicles.find(v => v.vehicleId === vehicleId);
    return vehicle ? `${vehicle.licensePlate}` : `Vehicle ${vehicleId}`;
  }

  // Create Reservation Methods
  showCreateReservationDialog(): void {
    this.createForm.reset({
      requiresDriver: false,
      status: VehicleReservationStatus.Requested
    });
    this.showCreateDialog = true;
  }

  hideCreateDialog(): void {
    this.showCreateDialog = false;
    this.createForm.reset();
  }

  createReservation(): void {
    if (this.createForm.invalid) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: 'Please fill in all required fields correctly'
      });
      return;
    }

    const formValue = this.createForm.value;

    // Validate destination is different from departure
    if (formValue.departure === formValue.destination) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: 'Destination must be different from departure'
      });
      return;
    }

    // Validate dates
    // Convert string to Date if needed
    const startDate = typeof formValue.startDate === 'string' ? new Date(formValue.startDate) : formValue.startDate;
    const endDate = typeof formValue.endDate === 'string' ? new Date(formValue.endDate) : formValue.endDate;
    if (!startDate || !endDate || isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: 'Please provide valid start and end dates'
      });
      return;
    }
    if (startDate >= endDate) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: 'Start date must be before end date'
      });
      return;
    }

    this.creating = true;
    const request: CreateVehicleReservationRequest = {
      requesterId: formValue.requesterId,
      vehicleId: formValue.vehicleId,
      requiresDriver: formValue.requiresDriver,
      departure: formValue.departure,
      destination: formValue.destination,
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
      status: formValue.status
    };

    this.reservationService.createVehicleReservation(request).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Reservation created successfully'
        });
        this.hideCreateDialog();
        this.loadReservations();
        this.creating = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to create reservation'
        });
        this.creating = false;
      }
    });
  }

  confirmDelete(reservationId: number): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete this reservation?',
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.deleteReservation(reservationId);
      }
    });
  }

  confirmApprove(reservationId: number): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to approve this reservation?',
      header: 'Approve Confirmation',
      icon: 'pi pi-check-circle',
      accept: () => {
        this.approveReservation(reservationId);
      }
    });
  }

  confirmReject(reservationId: number): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to reject this reservation?',
      header: 'Reject Confirmation',
      icon: 'pi pi-times-circle',
      accept: () => {
        this.rejectReservation(reservationId);
      }
    });
  }

  deleteReservation(reservationId: number): void {
    this.loading = true;
    this.reservationService.deleteVehicleReservation(reservationId).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Reservation deleted successfully'
        });
        this.loadReservations();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to delete reservation'
        });
        this.loading = false;
      }
    });
  }

  approveReservation(reservationId: number): void {
    this.loading = true;
    this.reservationService.approveReservation(reservationId).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Reservation approved successfully'
        });
        this.loadReservations();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to approve reservation'
        });
        this.loading = false;
      }
    });
  }

  rejectReservation(reservationId: number): void {
    this.loading = true;
    this.reservationService.rejectReservation(reservationId).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Reservation rejected successfully'
        });
        this.loadReservations();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to reject reservation'
        });
        this.loading = false;
      }
    });
  }

  // Computed properties for stats
  get requestedCount(): number {
    return this.reservations.filter(r => r.status === VehicleReservationStatus.Requested).length;
  }

  get approvedCount(): number {
    return this.reservations.filter(r => r.status === VehicleReservationStatus.Approved).length;
  }

  get rejectedCount(): number {
    return this.reservations.filter(r => r.status === VehicleReservationStatus.Rejected).length;
  }
} 