import { Component, NO_ERRORS_SCHEMA, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { MissionService } from '../../../core/services/mission.service';
import { SharedModule } from '../../../shared/shared.module';
import { MissionType, MissionStatus } from '../../../core/enums/mission.enums';
import { Mission, MissionGet } from '../../../core/models/Mission';
import { UserService } from '../../../core/services/User.service';
import { User } from '../../../core/models/User';
import { trigger, state, style, transition, animate, query, stagger } from '@angular/animations';
import { SiteService } from '../../../core/services/site.service';
import { Site } from '../../../core/models/Site';
import { PagedResponse } from '../../../core/models/PagedResponse';
import { MissionTasksComponent } from '../mission-tasks/mission-tasks.component';
import { TableModule } from 'primeng/table';
import { CheckboxModule } from 'primeng/checkbox';
import { Router } from '@angular/router';

// Custom validator for future dates
function futureDateValidator(control: AbstractControl): ValidationErrors | null {
  if (!control.value) {
    return null; // Allow empty values
  }
  
  const selectedDate = new Date(control.value);
  const now = new Date();
  
  // Remove time from comparison for date-only validation
  const selectedDateOnly = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), selectedDate.getDate());
  const nowDateOnly = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  
  if (selectedDateOnly < nowDateOnly) {
    return { pastDate: true };
  }
  
  return null;
}

@Component({
  selector: 'app-mission-list',
  standalone: true,
  templateUrl: './mission-list.component.html',
  styleUrls: ['./mission-list.component.scss'],
  imports: [SharedModule, MissionTasksComponent, TableModule, CheckboxModule],
  providers: [MessageService, ConfirmationService, MissionService, UserService, SiteService],
  schemas: [NO_ERRORS_SCHEMA],
  animations: [
    trigger('rowExpansionAnimation', [
      state('void', style({ height: '0px', opacity: 0, overflow: 'hidden' })),
      state('*', style({ height: '*', opacity: 1 })),
      transition('void <=> *', animate('300ms ease-in-out')),
    ]),
    trigger('filterCollapseAnimation', [
      state('void', style({ height: '0px', opacity: 0, paddingTop: '0', paddingBottom: '0', marginTop: '0', marginBottom: '0', overflow: 'hidden' })),
      state('*', style({ height: '*', opacity: 1, paddingTop: '*', paddingBottom: '*', marginTop: '*', marginBottom: '*' })),
      transition('void <=> *', [style({ overflow: 'hidden' }), animate('400ms cubic-bezier(0.4, 0.0, 0.2, 1)')]),
    ]),

    trigger('listAnimation', [
      transition('* => *', [
        query(':enter', [
          style({ opacity: 0, transform: 'translateX(-20px)' }),
          stagger(50, [
            animate('300ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
          ])
        ], { optional: true })
      ])
    ]),
    trigger('statusAnimation', [
      transition('* => *', [
        animate('200ms ease-in-out', style({ transform: 'scale(1.1)' })),
        animate('200ms ease-in-out', style({ transform: 'scale(1)' }))
      ])
    ])
  ],
})
export class MissionListComponent implements OnInit {
  @ViewChild('filterToggle') filterToggle: any;

  MissionType = MissionType;
  MissionStatus = MissionStatus;
  Math = Math;

  typeOptions: { label: string; value: MissionType }[] = Object.entries(MissionType)
    .filter(([key]) => isNaN(Number(key)))
    .map(([key, value]) => ({ label: key, value: value as MissionType }));

  statusOptions: { label: string; value: MissionStatus }[] = Object.entries(MissionStatus)
    .filter(([key]) => isNaN(Number(key)))
    .map(([key, value]) => ({ label: key, value: value as MissionStatus }));

  users: (User & { fullName: string })[] = [];
  missions: MissionGet[] = [];
  sites: Site[] = [];
  totalRecords: number = 0;
  loading: boolean = false;
  displayDialog: boolean = false;
  tasksModalVisible: boolean = false;
  selectedMission: MissionGet | null = null;
  missionForm: FormGroup;
  filterForm: FormGroup;
  isEditing: boolean = false;
  currentPage: number = 1;
  pageSize: number = 10;
  expandedRows: { [key: string]: boolean } = {};
  isFilterCollapsed: boolean = true;
  
  // Enhanced UI properties
  sortField: string = '';
  sortOrder: number = 1;
  filterCount: number = 0;
  showAdvancedFilters: boolean = false;
  selectedMissions: MissionGet[] = [];
  bulkActionsVisible: boolean = false;

  // Computed properties for template
  get pendingMissionsCount(): number {
    return this.missions.filter(m => m.status === MissionStatus.Requested).length;
  }

  get completedMissionsCount(): number {
    return this.missions.filter(m => m.status === MissionStatus.Completed).length;
  }

  get inProgressMissionsCount(): number {
    return this.missions.filter(m => m.status === MissionStatus.InProgress).length;
  }

  // Helper methods for template
  getSubmitButtonLabel(): string {
    return this.isEditing ? 'Update' : 'Create';
  }

  getDialogHeader(): string {
    return this.isEditing ? 'Edit Mission' : 'Add New Mission';
  }

  getMissionTypeError(): string {
    return this.getMissionFieldError('type');
  }

  getMissionStatusError(): string {
    return this.getMissionFieldError('status');
  }

  getMissionRequesterError(): string {
    return this.getMissionFieldError('requesterId');
  }

  getMissionDriverError(): string {
    return this.getMissionFieldError('driverId');
  }

  getMissionServiceError(): string {
    return this.getMissionFieldError('service');
  }

  getMissionReceiverError(): string {
    return this.getMissionFieldError('receiver');
  }

  getMissionDesiredDateError(): string {
    return this.getMissionFieldError('desiredDate');
  }

  // Get minimum date for calendar (today for new missions, null for editing)
  getMinDate(): Date | null {
    return this.isEditing ? null : new Date();
  }

  constructor(
    private fb: FormBuilder,
    private missionService: MissionService,
    private siteService: SiteService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private userService: UserService,
    private router: Router
  ) {
    this.missionForm = this.createMissionForm();
    this.filterForm = this.createFilterForm();
  }

  ngOnInit(): void {
    this.loadMissions();
    this.loadUsers();
    this.loadSites();
    this.setupFilterListener();
  }

  setupFilterListener(): void {
    this.filterForm.valueChanges.subscribe(() => {
      this.updateFilterCount();
    });
  }

  updateFilterCount(): void {
    const filterValues = this.filterForm.value;
    this.filterCount = Object.keys(filterValues).filter(key => 
      filterValues[key] !== null && filterValues[key] !== '' && filterValues[key] !== undefined
    ).length;
  }

  toggleFilterCollapse(): void {
    this.isFilterCollapsed = !this.isFilterCollapsed;
    if (!this.isFilterCollapsed) {
      setTimeout(() => {
        this.updateFilterCount();
      }, 100);
    }
  }

  toggleAdvancedFilters(): void {
    this.showAdvancedFilters = !this.showAdvancedFilters;
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

  loadSites(): void {
    this.siteService.getPagedSites(1, 500).subscribe({
      next: (response: PagedResponse<Site>) => {
        this.sites = response.data;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load sites' });
      },
    });
  }

  createMissionForm(): FormGroup {
    return this.fb.group({
      missionId: [0],
      type: [MissionType.Goods, Validators.required],
      requesterId: [null, [Validators.required, Validators.min(1)]],
      driverId: [null, [Validators.required, Validators.min(1)]],
      desiredDate: [null, [futureDateValidator]],
      service: ['', Validators.required],
      receiver: ['', Validators.required],
      status: [MissionStatus.Requested, Validators.required],
    });
  }

  createFilterForm(): FormGroup {
    return this.fb.group({
      type: [null],
      status: [null],
      driverId: [null],
      requesterId: [null],
      desiredDateStart: [null],
      desiredDateEnd: [null],
      systemDateStart: [null],
      systemDateEnd: [null],
      service: [''],
      receiver: [''],
    });
  }

  loadMissions(): void {
    this.loading = true;
    const filters = this.filterForm.value;
    this.missionService.getMissionsPaged(this.currentPage, this.pageSize, filters).subscribe({
      next: (response: { data: MissionGet[]; totalRecords: number }) => {
        this.missions = response.data;
        this.totalRecords = response.totalRecords;
        this.expandedRows = this.missions.reduce((acc, mission) => {
          acc[mission.missionId] = !!this.expandedRows[mission.missionId];
          return acc;
        }, {} as { [key: string]: boolean });
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load missions' });
        this.loading = false;
      },
    });
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.loadMissions();
    this.messageService.add({ 
      severity: 'info', 
      summary: 'Filter Applied',
      detail: 'filters have been applied' 
    });
  }

  resetFilter(): void {
    this.filterForm.reset();
    this.filterCount = 0;
    this.currentPage = 1;
    this.loadMissions();
    this.messageService.add({ 
      severity: 'info', 
      summary: 'Filter Reset', 
      detail: 'All filters have been cleared' 
    });
  }

  onPageChange(event: any): void {
    this.currentPage = event.page + 1;
    this.pageSize = event.rows;
    this.loadMissions();
  }

  onPageSizeChange(event: any): void {
    this.pageSize = parseInt(event.target.value);
    this.currentPage = 1; // Reset to first page when changing page size
    this.loadMissions();
  }

  onSort(event: any): void {
    this.sortField = event.field;
    this.sortOrder = event.order;
    // Implement sorting logic here if needed
  }

  showAddEditDialog(mission?: Mission): void {
    // Force close any existing dialogs first
    this.displayDialog = false;
    this.tasksModalVisible = false;
    
    // Use setTimeout to ensure the previous dialog is fully closed
    setTimeout(() => {
      this.resetMissionForm();
      if (mission) {
        this.isEditing = true;
        // For editing, we need to handle past dates differently
        const formData = {
          ...mission,
          systemDate: mission.systemDate ? new Date(mission.systemDate) : null,
          desiredDate: mission.desiredDate ? this.formatDateForInput(new Date(mission.desiredDate)) : null,
        };
        setTimeout(() => {
          this.missionForm.patchValue(formData);
        }, 0);
      } else {
        this.isEditing = false;
        // For add, ensure desiredDate is null
        setTimeout(() => {
          this.missionForm.patchValue({ desiredDate: null });
        }, 0);
      }
      
      // Force the dialog to open
      this.displayDialog = true;
    }, 100);
  }

  showTasksModal(mission: MissionGet): void {
    this.selectedMission = mission;
    this.tasksModalVisible = true;
  }

  viewMissionSheet(mission: MissionGet): void {
    this.router.navigate(['/missions/sheet', mission.missionId]);
  }

  onMissionSubmit(): void {
    if (this.missionForm.invalid) {
      this.markFormTouched(this.missionForm);
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fix the form errors' });
      return;
    }

    const mission: Mission = { ...this.missionForm.value };
    if (mission.desiredDate) {
      const localDate = new Date(mission.desiredDate);
      mission.desiredDate = new Date(
        Date.UTC(
          localDate.getFullYear(),
          localDate.getMonth(),
          localDate.getDate(),
          localDate.getHours(),
          localDate.getMinutes(),
          localDate.getSeconds()
        )
      );
    }
    this.loading = true;

    const handleError = (err: any): void => {
      let errorMessage = 'Operation failed';
      
      if (err?.error?.errors) {
        // Handle validation errors from backend
        const validationErrors = err.error.errors;
        const errorMessages: string[] = [];
        
        Object.keys(validationErrors).forEach(key => {
          if (Array.isArray(validationErrors[key])) {
            errorMessages.push(...validationErrors[key]);
          } else {
            errorMessages.push(validationErrors[key]);
          }
        });
        
        if (errorMessages.length > 0) {
          errorMessage = errorMessages.join(', ');
        }
      } else if (err?.error?.Message) {
        errorMessage = err.error.Message;
      } else if (err?.message) {
        errorMessage = err.message;
      }
      
      this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
      this.loading = false;
      console.error('Mission operation error:', err);
    };

    if (this.isEditing) {
      this.missionService.updateMission(mission.missionId, mission).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Mission updated successfully' });
          this.displayDialog = false;
          this.loadMissions();
          this.loading = false;
        },
        error: handleError,
      });
    } else {
      this.missionService.createMission(mission).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Mission created successfully' });
          this.displayDialog = false;
          this.loadMissions();
          this.loading = false;
        },
        error: handleError,
      });
    }
  }

  confirmDeleteMission(mission: MissionGet): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete mission "${mission.service}"?`,
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteMission(mission.missionId),
    });
  }

  deleteMission(id: number): void {
    this.loading = true;
    this.missionService.deleteMission(id).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Mission deleted successfully' });
        this.loadMissions();
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete mission' });
        this.loading = false;
      },
    });
  }

  resetMissionForm(): void {
    this.missionForm.reset({
      missionId: 0,
      type: MissionType.Goods,
      requesterId: null,
      driverId: null,
      desiredDate: null,
      service: '',
      receiver: '',
      status: MissionStatus.Requested,
    });
    
    // Restore the future date validator
    const desiredDateControl = this.missionForm.get('desiredDate');
    if (desiredDateControl) {
      desiredDateControl.setValidators([futureDateValidator]);
      desiredDateControl.updateValueAndValidity();
    }
    
    this.isEditing = false;
  }

  markFormTouched(form: FormGroup): void {
    Object.values(form.controls).forEach((control) => control.markAsTouched());
  }

  getMissionFieldError(field: string): string {
    const control = this.missionForm.get(field);
    if (control?.touched && control?.errors) {
      if (control.errors['required']) return 'This field is required';
      if (control.errors['min']) return 'Please select a valid option';
      if (control.errors['pastDate']) return 'Desired date cannot be in the past';
    }
    return '';
  }

  getTypeLabel(type: MissionType): string {
    return MissionType[type] || 'Unknown';
  }

  getStatusLabel(status: MissionStatus): string {
    return MissionStatus[status] || 'Unknown';
  }

  getTypeBadgeClass(type: MissionType): string {
    switch (type) {
      case MissionType.Goods:
        return 'bg-blue-100 text-blue-800 border-blue-200';
      case MissionType.Financial:
        return 'bg-green-100 text-green-800 border-green-200';
      case MissionType.Administrative:
        return 'bg-purple-100 text-purple-800 border-purple-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  }

  getStatusBadgeClass(status: MissionStatus): string {
    switch (status) {
      case MissionStatus.Requested:
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case MissionStatus.Approved:
        return 'bg-blue-100 text-blue-800 border-blue-200';
      case MissionStatus.Planned:
        return 'bg-indigo-100 text-indigo-800 border-indigo-200';
      case MissionStatus.InProgress:
        return 'bg-orange-100 text-orange-800 border-orange-200';
      case MissionStatus.Completed:
        return 'bg-green-100 text-green-800 border-green-200';
      case MissionStatus.Rejected:
        return 'bg-red-100 text-red-800 border-red-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  }

  onRowToggle(missionId: number): void {
    this.expandedRows[missionId] = !this.expandedRows[missionId];
  }

  // Enhanced methods for better UX
  getMissionPriority(mission: MissionGet): 'high' | 'medium' | 'low' {
    const daysUntilDesired = mission.desiredDate ? 
      Math.ceil((new Date(mission.desiredDate).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24)) : 0;
    
    if (daysUntilDesired <= 1) return 'high';
    if (daysUntilDesired <= 3) return 'medium';
    return 'low';
  }

  getPriorityBadgeClass(priority: 'high' | 'medium' | 'low'): string {
    switch (priority) {
      case 'high':
        return 'bg-red-100 text-red-800 border-red-200';
      case 'medium':
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'low':
        return 'bg-green-100 text-green-800 border-green-200';
    }
  }

  getPriorityIcon(priority: 'high' | 'medium' | 'low'): string {
    switch (priority) {
      case 'high':
        return 'pi pi-exclamation-triangle';
      case 'medium':
        return 'pi pi-clock';
      case 'low':
        return 'pi pi-check-circle';
    }
  }

  // Bulk actions
  onSelectionChange(): void {
    this.bulkActionsVisible = this.selectedMissions.length > 0;
  }

  bulkUpdateStatus(status: MissionStatus): void {
    if (this.selectedMissions.length === 0) return;

    this.confirmationService.confirm({
      message: `Are you sure you want to update ${this.selectedMissions.length} missions to ${this.getStatusLabel(status)}?`,
      header: 'Bulk Update Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        // Implement bulk update logic
        this.messageService.add({ 
          severity: 'success', 
          summary: 'Bulk Update', 
          detail: `${this.selectedMissions.length} missions updated successfully` 
        });
        this.selectedMissions = [];
        this.bulkActionsVisible = false;
        this.loadMissions();
      }
    });
  }

  // Export functionality
  exportMissions(): void {
    // Implement export logic (CSV, Excel, etc.)
    this.messageService.add({ 
      severity: 'info', 
      summary: 'Export', 
      detail: 'Export functionality will be implemented soon' 
    });
  }

  // Quick actions
  duplicateMission(mission: MissionGet): void {
    // Create a new mission object with copied data
    const duplicatedMission: Mission = {
      missionId: 0, // This ensures it's treated as a new mission
      type: mission.type,
      requesterId: mission.requesterId,
      driverId: mission.driverId,
      desiredDate: mission.desiredDate,
      service: `${mission.service} (Copy)`,
      receiver: mission.receiver,
      status: MissionStatus.Requested, // Always start with Requested status
      systemDate: new Date()
    };
    
    // Force close any existing dialogs first
    this.displayDialog = false;
    this.tasksModalVisible = false;
    
    // Use setTimeout to ensure the previous dialog is fully closed
    setTimeout(() => {
      // Reset form and set to create mode
      this.resetMissionForm();
      this.isEditing = false;
      
      // Populate form with duplicated data, formatting the date properly
      const formData = {
        ...duplicatedMission,
        desiredDate: duplicatedMission.desiredDate ? this.formatDateForInput(new Date(duplicatedMission.desiredDate)) : null,
      };
      this.missionForm.patchValue(formData);
      
      // Show the dialog
      this.displayDialog = true;
    }, 100);
  }

  // Search functionality
  searchMissions(query: string): void {
    // Implement search logic
    console.log('Searching for:', query);
  }

  // Track by function for performance
  trackByMissionId(index: number, mission: MissionGet): number {
    return mission.missionId;
  }

  // Helper for search input event
  searchMissionsInput(event: Event) {
    const value = (event.target && (event.target as HTMLInputElement).value) || '';
    this.searchMissions(value);
  }

  // Helper for displaying person name
  getPersonName(person: any): string {
    if (person && typeof person === 'object' && 'lastName' in person && 'firstName' in person) {
      return `${person.lastName} ${person.firstName}`;
    }
    return person || '';
  }



  // Get pagination info for display
  getPaginationInfo(): string {
    const start = (this.currentPage - 1) * this.pageSize + 1;
    const end = Math.min(this.currentPage * this.pageSize, this.totalRecords);
    return `${start} to ${end} of ${this.totalRecords} entries`;
  }

  // Helper method to format date for datetime-local input
  private formatDateForInput(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

   // Navigate to manage tasks
   navigateToTasks(missionId: number): void {
    // Navigate to mission tasks route
    this.router.navigate([`/missions/tasks/${missionId}`]);
  }
}