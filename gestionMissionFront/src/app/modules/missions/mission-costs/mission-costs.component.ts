import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { DialogModule } from 'primeng/dialog';
import { FileUploadModule } from 'primeng/fileupload';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { RippleModule } from 'primeng/ripple';
import { HttpClient } from '@angular/common/http';
import { MissionCostService, MissionCostFilters } from '../../../core/services/mission-cost.service';
import { MissionCost, MissionCostType } from '../../../core/models/MissionCost';
import { MissionService } from '../../../core/services/mission.service';
import { MissionGet } from '../../../core/models/Mission';
import { PagedResponse } from '../../../core/models/PagedResponse';

@Component({
  selector: 'app-mission-costs',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    CalendarModule,
    DialogModule,
    FileUploadModule,
    ToastModule,
    ConfirmDialogModule,
    CardModule,
    TagModule,
    TooltipModule,
    RippleModule
  ],
  providers: [
    MessageService,
    ConfirmationService,
    MissionCostService,
    MissionService,
    HttpClient
  ],
  templateUrl: './mission-costs.component.html',
  styleUrls: ['./mission-costs.component.scss']
})
export class MissionCostsComponent implements OnInit {
  costs: MissionCost[] = [];
  missions: MissionGet[] = [];
  loading: boolean = false;
  displayDialog: boolean = false;
  isEditMode: boolean = false;
  selectedCost: MissionCost | null = null;
  uploadedFiles: File[] = [];
  totalCost: number = 0;
  maxDate: Date = new Date();

  // Photo gallery properties
  showPhotoGallery: boolean = false;
  selectedPhotos: string[] = [];
  currentPhotoIndex: number = 0;

  costForm: FormGroup;
  filterForm: FormGroup;
  MissionCostType = MissionCostType;

  typeOptions = [
    { label: 'Fuel', value: MissionCostType.Fuel },
    { label: 'Toll', value: MissionCostType.Toll },
    { label: 'Maintenance', value: MissionCostType.Maintenance }
  ];

  // Pagination and filtering properties
  currentPage: number = 1;
  itemsPerPage: number = 10;
  totalItems: number = 0;
  filters: MissionCostFilters = {};

  constructor(
    private fb: FormBuilder,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private missionCostService: MissionCostService,
    private missionService: MissionService
  ) {
    this.costForm = this.createCostForm();
    this.filterForm = this.createFilterForm();
  }

  ngOnInit() {
    this.loadCosts();
    this.loadMissions();
  }

  createCostForm(): FormGroup {
    return this.fb.group({
      missionId: [null, [Validators.required, Validators.min(1)]],
      type: [null, [Validators.required]],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      date: [null, [Validators.required]]
    });
  }

  createFilterForm(): FormGroup {
    return this.fb.group({
      missionId: [null],
      type: [null],
      minAmount: [null],
      maxAmount: [null],
      dateStart: [null],
      dateEnd: [null]
    });
  }

  getTypeLabel(type: MissionCostType): string {
    switch (type) {
      case MissionCostType.Fuel:
        return 'Fuel Expenses';
      case MissionCostType.Toll:
        return 'Toll Fees';
      case MissionCostType.Maintenance:
        return 'Vehicle Maintenance';
      default:
        return 'Unknown Type';
    }
  }

  loadCosts() {
    this.loading = true;
    
    // Build filters from form values
    const filterValues = this.filterForm.value;
    const filters: MissionCostFilters = {
      pageNumber: this.currentPage,
      pageSize: this.itemsPerPage,
      missionId: filterValues.missionId || undefined,
      type: filterValues.type || undefined,
      minAmount: filterValues.minAmount || undefined,
      maxAmount: filterValues.maxAmount || undefined,
      dateStart: filterValues.dateStart || undefined,
      dateEnd: filterValues.dateEnd || undefined
    };

    this.missionCostService.getPaginatedMissionCosts(filters).subscribe({
      next: (response: PagedResponse<MissionCost>) => {
        this.costs = response.data;
        this.totalItems = response.totalRecords;
        this.calculateTotalCost();
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load mission costs'
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

  calculateTotalCost() {
    this.totalCost = this.costs.reduce((sum, cost) => sum + cost.amount, 0);
  }

  showAddDialog() {
    this.isEditMode = false;
    this.selectedCost = null;
    this.uploadedFiles = [];
    this.costForm.reset();
    this.displayDialog = true;
  }

  showEditDialog(cost: MissionCost) {
    this.isEditMode = true;
    this.selectedCost = cost;
    this.uploadedFiles = [];
    
    this.costForm.patchValue({
      missionId: cost.missionId,
      type: cost.type,
      amount: cost.amount,
      date: this.formatDateForInput(cost.date)
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
          detail: 'Maximum 10 photos allowed'
        });
        return;
      }

      // Validate file types and sizes
      for (let file of files) {
        if (!this.isValidImageFile(file)) {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: `Invalid file: ${file.name}. Only JPEG, PNG, GIF files up to 10MB are allowed.`
          });
          return;
        }
      }

      this.uploadedFiles.push(...files);
    }
  }

  isValidImageFile(file: File): boolean {
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
    const maxSize = 10 * 1024 * 1024; // 10MB

    return validTypes.includes(file.type) && file.size <= maxSize;
  }

  removeFile(file: File) {
    const index = this.uploadedFiles.indexOf(file);
    if (index > -1) {
      this.uploadedFiles.splice(index, 1);
    }
  }

  saveCost() {
    if (this.costForm.valid && this.validateForm()) {
      const formValue = this.costForm.value;
      const date = new Date(formValue.date);

      this.loading = true;

      if (this.isEditMode && this.selectedCost) {
        // Update existing cost
        this.missionCostService.updateMissionCost(
          this.selectedCost.costId,
          formValue.missionId,
          formValue.type,
          formValue.amount,
          date.toISOString(),
          this.uploadedFiles
        ).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Mission cost updated successfully'
            });
            this.loadCosts();
            this.closeDialog();
            this.loading = false;
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to update mission cost'
            });
            this.loading = false;
          }
        });
      } else {
        // Create new cost
        this.missionCostService.createMissionCost(
          formValue.missionId,
          formValue.type,
          formValue.amount,
          date.toISOString(),
          this.uploadedFiles
        ).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Mission cost created successfully'
            });
            this.loadCosts();
            this.closeDialog();
            this.loading = false;
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to create mission cost'
            });
            this.loading = false;
          }
        });
      }
    }
  }

  deleteCost(cost: MissionCost) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete this ${this.getTypeLabel(cost.type).toLowerCase()} cost of $${cost.amount}?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.loading = true;
        this.missionCostService.deleteMissionCost(cost.costId).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Mission cost deleted successfully'
            });
            this.loadCosts();
            this.loading = false;
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to delete mission cost'
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

  getTypeBadgeClass(type: MissionCostType): string {
    switch (type) {
      case MissionCostType.Fuel:
        return 'bg-orange-100 text-orange-700';
      case MissionCostType.Toll:
        return 'bg-blue-100 text-blue-700';
      case MissionCostType.Maintenance:
        return 'bg-purple-100 text-purple-700';
      default:
        return 'bg-gray-100 text-gray-700';
    }
  }

  getTypeSeverity(type: MissionCostType): 'success' | 'secondary' | 'info' | 'warning' | 'danger' | 'contrast' {
    switch (type) {
      case MissionCostType.Fuel:
        return 'warning';
      case MissionCostType.Toll:
        return 'info';
      case MissionCostType.Maintenance:
        return 'secondary';
      default:
        return 'secondary';
    }
  }

  viewPhotos(cost: MissionCost) {
    if (cost.receiptPhotoUrls && cost.receiptPhotoUrls.length > 0) {
      this.selectedPhotos = cost.receiptPhotoUrls;
      this.currentPhotoIndex = 0;
      this.showPhotoGallery = true;
    } else {
      this.messageService.add({
        severity: 'info',
        summary: 'Info',
        detail: 'No receipt photos available'
      });
    }
  }

  getImageUrl(photoUrl: string): string {
    return this.missionCostService.getImageUrl(photoUrl);
  }

  nextPhoto() {
    if (this.currentPhotoIndex < this.selectedPhotos.length - 1) {
      this.currentPhotoIndex++;
    }
  }

  previousPhoto() {
    if (this.currentPhotoIndex > 0) {
      this.currentPhotoIndex--;
    }
  }

  closePhotoGallery() {
    this.showPhotoGallery = false;
    this.selectedPhotos = [];
    this.currentPhotoIndex = 0;
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

  // Additional API demonstration methods
  getCostById(costId: number) {
    this.missionCostService.getMissionCostById(costId).subscribe({
      next: (cost) => {
        this.messageService.add({
          severity: 'info',
          summary: 'Cost Details',
          detail: `Cost ID ${cost.costId}: ${this.getTypeLabel(cost.type)} - $${cost.amount}`
        });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to get cost details'
        });
      }
    });
  }

  getCostsByMission(missionId: number) {
    this.missionCostService.getMissionCostsByMissionId(missionId).subscribe({
      next: (costs) => {
        this.messageService.add({
          severity: 'info',
          summary: 'Mission Costs',
          detail: `Found ${costs.length} costs for mission ${missionId}`
        });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to get mission costs'
        });
      }
    });
  }

  getTotalCostForMission(missionId: number) {
    this.missionCostService.getTotalCostByMissionId(missionId).subscribe({
      next: (total) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Total Cost',
          detail: `Total cost for mission ${missionId}: $${total.toFixed(2)}`
        });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to get total cost'
        });
      }
    });
  }

  // Enhanced validation methods
  validateForm(): boolean {
    if (this.costForm.invalid) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: 'Please fill in all required fields correctly'
      });
      return false;
    }
    return true;
  }

  // Pagination methods
  onPageChange(event: any) {
    this.currentPage = Math.floor(event.first / event.rows) + 1;
    this.itemsPerPage = event.rows;
    this.loadCosts();
  }

  // Filtering methods
  applyFilters() {
    this.currentPage = 1; // Reset to first page when applying filters
    this.loadCosts();
  }

  clearFilters() {
    this.filterForm.reset();
    this.currentPage = 1;
    this.loadCosts();
  }

  // Get total pages for pagination
  get totalPages(): number {
    return Math.ceil(this.totalItems / this.itemsPerPage);
  }

  // Check if there are active filters
  get hasActiveFilters(): boolean {
    const values = this.filterForm.value;
    return !!(values.missionId || values.type || values.minAmount || values.maxAmount || values.dateStart || values.dateEnd);
  }

  // Dialog management
  closeDialog(): void {
    this.displayDialog = false;
  }
} 