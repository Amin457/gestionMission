import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { Incident, IncidentType, IncidentStatus } from '../../../core/models/Incident';
import { IncidentService, IncidentFilters } from '../../../core/services/incident.service';
import { MissionService } from '../../../core/services/mission.service';
import { MissionGet } from '../../../core/models/Mission';
import { PagedResponse } from '../../../core/models/PagedResponse';

// PrimeNG Components
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { DropdownModule } from 'primeng/dropdown';
import { FileUploadModule } from 'primeng/fileupload';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-incidents',
  templateUrl: './incidents.component.html',
  styleUrls: ['./incidents.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    DialogModule,
    TableModule,
    DropdownModule,
    FileUploadModule,
    InputTextModule,
    InputTextareaModule,
    TagModule,
    ToastModule,
    ConfirmDialogModule,
    TooltipModule
  ],
  providers: [MessageService, ConfirmationService]
})
export class IncidentsComponent implements OnInit {
  incidents: Incident[] = [];
  missions: MissionGet[] = [];
  loading: boolean = false;
  displayDialog: boolean = false;
  isEditMode: boolean = false;
  selectedIncident: Incident | null = null;
  uploadedFiles: File[] = [];
  showFileGallery: boolean = false;
  selectedFiles: string[] = [];
  currentFileIndex: number = 0;

  incidentForm: FormGroup;
  filterForm: FormGroup;
  IncidentType = IncidentType;
  IncidentStatus = IncidentStatus;

  typeOptions = [
    { label: 'Delay', value: IncidentType.Delay },
    { label: 'Breakdown', value: IncidentType.Breakdown },
    { label: 'Logistics Issue', value: IncidentType.LogisticsIssue }
  ];

  statusOptions = [
    { label: 'Reported', value: IncidentStatus.Reported },
    { label: 'In Progress', value: IncidentStatus.InProgress },
    { label: 'Resolved', value: IncidentStatus.Resolved }
  ];

  // Pagination and filtering properties
  currentPage: number = 1;
  itemsPerPage: number = 10;
  totalItems: number = 0;
  filters: IncidentFilters = {};

  constructor(
    private fb: FormBuilder,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private incidentService: IncidentService,
    private missionService: MissionService
  ) {
    this.incidentForm = this.createIncidentForm();
    this.filterForm = this.createFilterForm();
  }

  ngOnInit() {
    this.loadIncidents();
    this.loadMissions();
  }

  createIncidentForm(): FormGroup {
    return this.fb.group({
      missionId: [null, [Validators.required, Validators.min(1)]],
      type: [null, [Validators.required]],
      description: [null, [Validators.required, Validators.maxLength(500)]],
      status: [IncidentStatus.Reported, [Validators.required]],
      reportDate: [null, [Validators.required]]
    });
  }

  createFilterForm(): FormGroup {
    return this.fb.group({
      missionId: [null],
      type: [null],
      status: [null],
      reportDateStart: [null],
      reportDateEnd: [null]
    });
  }

  getTypeLabel(type: IncidentType): string {
    switch (type) {
      case IncidentType.Delay:
        return 'Delay';
      case IncidentType.Breakdown:
        return 'Breakdown';
      case IncidentType.LogisticsIssue:
        return 'Logistics Issue';
      default:
        return 'Unknown Type';
    }
  }

  getStatusLabel(status: IncidentStatus): string {
    switch (status) {
      case IncidentStatus.Reported:
        return 'Reported';
      case IncidentStatus.InProgress:
        return 'In Progress';
      case IncidentStatus.Resolved:
        return 'Resolved';
      default:
        return 'Unknown Status';
    }
  }

  loadIncidents() {
    this.loading = true;
    
    // Build filters from form values
    const filterValues = this.filterForm.value;
    const filters: IncidentFilters = {
      pageNumber: this.currentPage,
      pageSize: this.itemsPerPage,
      missionId: filterValues.missionId || undefined,
      type: filterValues.type || undefined,
      status: filterValues.status || undefined,
      reportDateStart: filterValues.reportDateStart || undefined,
      reportDateEnd: filterValues.reportDateEnd || undefined
    };

    this.incidentService.getPaginatedIncidents(filters).subscribe({
      next: (response: PagedResponse<Incident>) => {
        this.incidents = response.data;
        this.totalItems = response.totalRecords;
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load incidents'
        });
        this.loading = false;
      }
    });
  }

  loadMissions() {
    this.missionService.getMissionsPaged(1, 1000, {}).subscribe({
      next: (response) => {
        this.missions = response.data;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load missions'
        });
      }
    });
  }

  showAddDialog() {
    this.isEditMode = false;
    this.selectedIncident = null;
    this.uploadedFiles = [];
    this.incidentForm.reset();
    this.incidentForm.patchValue({
      status: IncidentStatus.Reported
    });
    this.displayDialog = true;
  }

  showEditDialog(incident: Incident) {
    this.isEditMode = true;
    this.selectedIncident = incident;
    this.uploadedFiles = [];
    this.incidentForm.patchValue({
      missionId: incident.missionId,
      type: incident.type,
      description: incident.description,
      reportDate: this.formatDateForInput(incident.reportDate),
      status: incident.status
    });
    this.displayDialog = true;
  }

  onFileSelect(event: any) {
    const files = event.files;
    if (files && files.length > 0) {
      // Validate file count
      if (this.uploadedFiles.length + files.length > 10) {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Maximum 10 files allowed'
        });
        return;
      }

      // Validate file types and sizes
      for (let file of files) {
        if (!this.isValidFile(file)) {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: `Invalid file: ${file.name}. Only JPEG, PNG, GIF, PDF, DOC, DOCX files up to 10MB are allowed.`
          });
          return;
        }
      }

      this.uploadedFiles.push(...files);
    }
  }

  isValidFile(file: File): boolean {
    const validTypes = [
      'image/jpeg', 'image/jpg', 'image/png', 'image/gif',
      'application/pdf', 'application/msword', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
    ];
    const maxSize = 10 * 1024 * 1024; // 10MB

    return validTypes.includes(file.type) && file.size <= maxSize;
  }

  removeFile(file: File) {
    const index = this.uploadedFiles.indexOf(file);
    if (index > -1) {
      this.uploadedFiles.splice(index, 1);
    }
  }

  saveIncident() {
    if (this.incidentForm.valid && this.validateForm()) {
      const formValue = this.incidentForm.value;
      const date = new Date(formValue.reportDate);

      this.loading = true;

      if (this.isEditMode && this.selectedIncident) {
        // Update existing incident
        this.incidentService.updateIncident(
          this.selectedIncident.incidentId,
          formValue.missionId,
          formValue.type,
          formValue.description,
          date.toISOString(),
          formValue.status,
          this.uploadedFiles
        ).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Incident updated successfully'
            });
            this.loadIncidents();
            this.closeDialog();
            this.loading = false;
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to update incident'
            });
            this.loading = false;
          }
        });
      } else {
        // Create new incident
        this.incidentService.createIncident(
          formValue.missionId,
          formValue.type,
          formValue.description,
          date.toISOString(),
          formValue.status,
          this.uploadedFiles
        ).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Incident created successfully'
            });
            this.loadIncidents();
            this.closeDialog();
            this.loading = false;
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to create incident'
            });
            this.loading = false;
          }
        });
      }
    }
  }

  deleteIncident(incident: Incident) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete this ${this.getTypeLabel(incident.type).toLowerCase()} incident?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.loading = true;
        this.incidentService.deleteIncident(incident.incidentId).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Incident deleted successfully'
            });
            this.loadIncidents();
            this.loading = false;
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to delete incident'
            });
            this.loading = false;
          }
        });
      }
    });
  }

  getMissionTitle(missionId: number): string {
    const mission = this.missions.find(m => m.missionId === missionId);
    return mission ? `${mission.service} - ${mission.requester}` : `Mission ${missionId}`;
  }

  getTypeSeverity(type: IncidentType): 'success' | 'secondary' | 'info' | 'warning' | 'danger' | 'contrast' {
    switch (type) {
      case IncidentType.Delay:
        return 'warning';
      case IncidentType.Breakdown:
        return 'danger';
      case IncidentType.LogisticsIssue:
        return 'info';
      default:
        return 'secondary';
    }
  }

  getStatusSeverity(status: IncidentStatus): 'success' | 'secondary' | 'info' | 'warning' | 'danger' | 'contrast' {
    switch (status) {
      case IncidentStatus.Reported:
        return 'warning';
      case IncidentStatus.InProgress:
        return 'info';
      case IncidentStatus.Resolved:
        return 'success';
      default:
        return 'secondary';
    }
  }

  viewFiles(incident: Incident) {
    if (incident.incidentDocsUrls && incident.incidentDocsUrls.length > 0) {
      this.selectedFiles = incident.incidentDocsUrls;
      this.currentFileIndex = 0;
      this.showFileGallery = true;
    } else {
      this.messageService.add({
        severity: 'info',
        summary: 'Info',
        detail: 'No incident documents available'
      });
    }
  }

  getFileUrl(fileUrl: string): string {
    return this.incidentService.getFileUrl(fileUrl);
  }

  nextFile() {
    if (this.currentFileIndex < this.selectedFiles.length - 1) {
      this.currentFileIndex++;
    }
  }

  previousFile() {
    if (this.currentFileIndex > 0) {
      this.currentFileIndex--;
    }
  }

  closeFileGallery() {
    this.showFileGallery = false;
    this.selectedFiles = [];
    this.currentFileIndex = 0;
  }

  // Helper method to format date for native date input
  formatDateForInput(date: string | Date): string {
    const dateObj = new Date(date);
    return dateObj.toISOString().split('T')[0];
  }

  // Helper method to get current date in YYYY-MM-DD format
  getCurrentDateString(): string {
    return new Date().toISOString().split('T')[0];
  }

  // Helper method to check if file is an image
  isImageFile(fileUrl: string): boolean {
    const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif'];
    const extension = fileUrl.toLowerCase().split('.').pop();
    return imageExtensions.includes(`.${extension}`);
  }

  // Computed properties to avoid template parser errors
  get inProgressCount(): number {
    return this.incidents.filter(i => i.status === IncidentStatus.InProgress).length;
  }

  get resolvedCount(): number {
    return this.incidents.filter(i => i.status === IncidentStatus.Resolved).length;
  }

  // Dialog management methods
  closeDialog(): void {
    this.displayDialog = false;
  }

  selectFile(index: number): void {
    this.currentFileIndex = index;
  }

  // Enhanced validation methods
  validateForm(): boolean {
    const formValue = this.incidentForm.value;
    
    // Check if mission exists
    if (!this.missions.find(m => m.missionId === formValue.missionId)) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: 'Selected mission does not exist'
      });
      return false;
    }

    // Check description length
    if (formValue.description.length > 500) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: 'Description cannot exceed 500 characters'
      });
      return false;
    }

    // Check date validation
    const date = new Date(formValue.reportDate);
    if (date > new Date()) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: 'Report date cannot be in the future'
      });
      return false;
    }

    // Check file validation
    if (this.uploadedFiles.length > 10) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: 'Maximum 10 files allowed'
      });
      return false;
    }

    return true;
  }

  // Check if there are active filters
  get hasActiveFilters(): boolean {
    const values = this.filterForm.value;
    return !!(values.missionId || values.type || values.status || values.reportDateStart || values.reportDateEnd);
  }

  // Pagination methods
  onPageChange(event: any) {
    this.currentPage = Math.floor(event.first / event.rows) + 1;
    this.itemsPerPage = event.rows;
    this.loadIncidents();
  }

  // Filtering methods
  applyFilters() {
    this.currentPage = 1; // Reset to first page when applying filters
    this.loadIncidents();
  }

  clearFilters() {
    this.filterForm.reset();
    this.currentPage = 1;
    this.loadIncidents();
  }

  // Get total pages for pagination
  get totalPages(): number {
    return Math.ceil(this.totalItems / this.itemsPerPage);
  }
} 