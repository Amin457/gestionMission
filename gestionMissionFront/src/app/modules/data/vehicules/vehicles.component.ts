import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { Vehicle, VehicleFilter } from '../../../core/models/Vehicle';
import { PagedResponse } from '../../../core/models/PagedResponse';
import { VehicleService } from '../../../core/services/vehicle.service';
import { SharedModule } from '../../../shared/shared.module';
import { VehicleType, VehicleTypeLabel } from '../../../core/enums/VehicleType';
import { FileUpload } from 'primeng/fileupload';

@Component({
  selector: 'app-vehicles',
  standalone: true,
  templateUrl: './vehicles.component.html',
  styleUrls: ['./vehicles.component.scss'],
  imports: [SharedModule],
  providers: [MessageService, ConfirmationService, VehicleService]
})
export class VehiclesComponent implements OnInit {
  @ViewChild('fileUpload') fileUpload!: FileUpload;

  // Data properties
  vehicles: Vehicle[] = [];
  totalRecords = 0;
  loading = false;
  displayDialog = false;
  displayImageDialog = false;
  selectedVehicle: Vehicle | null = null;
  selectedImageIndex = 0;

  // Form properties
  vehicleForm: FormGroup;
  filterForm: FormGroup;
  isEditing = false;
  currentPage: number = 1;
  pageSize: number = 10;

  // Filter properties
  showFilters = false;
  appliedFilters: VehicleFilter = {};
  filterCount = 0;

  // Image properties
  selectedPhotos: File[] = [];
  existingPhotos: string[] = [];
  photosToKeep: string[] = [];

  // UI properties
  viewMode: 'grid' | 'table' = 'grid';
  sortField = '';
  sortOrder = 1;

  // Enums and options
  VehicleType = VehicleType;
  VehicleTypeOptions = Object.entries(VehicleTypeLabel).map(([value, label]) => ({
    label,
    value: Number(value)
  }));

  allTypeOptions = [
    { label: 'All Types', value: null },
    ...this.VehicleTypeOptions
  ];

  availabilityOptions = [
    { label: 'All', value: null },
    { label: 'Available', value: true },
    { label: 'Unavailable', value: false }
  ];

  constructor(
    private fb: FormBuilder,
    private vehicleService: VehicleService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {
    this.vehicleForm = this.createVehicleForm();
    this.filterForm = this.createFilterForm();
  }

  ngOnInit(): void {
    this.loadVehicles();
  }

  createVehicleForm(): FormGroup {
    return this.fb.group({
      vehicleId: [0],
      type: [null, [Validators.required]],
      licensePlate: ['', [Validators.required, Validators.maxLength(50), Validators.pattern('^[A-Za-z0-9\\s-]+$')]],
      availability: [true],
      maxCapacity: [0, [Validators.required, Validators.min(1)]]
    });
  }

  createFilterForm(): FormGroup {
    return this.fb.group({
      type: [null],
      availability: [null],
      licensePlate: [''],
      minCapacity: [null],
      maxCapacity: [null]
    });
  }

  loadVehicles(): void {
    this.loading = true;
    this.vehicleService.getPagedVehicles(this.currentPage, this.pageSize, this.appliedFilters).subscribe({
      next: (response: PagedResponse<Vehicle>) => {
        this.vehicles = response.data;
        this.totalRecords = response.totalRecords;
        this.loading = false;
      },
      error: err => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load vehicles' });
        this.loading = false;
        console.error(err);
      }
    });
  }

  onPageChange(event: any): void {
    this.currentPage = event.page + 1;
    this.pageSize = event.rows;
    this.loadVehicles();
  }

  onSort(event: any): void {
    this.sortField = event.field;
    this.sortOrder = event.order;
    this.loadVehicles();
  }

  applyFilters(): void {
    const filterValues = this.filterForm.value;
    this.appliedFilters = {};
    this.filterCount = 0;

    Object.keys(filterValues).forEach(key => {
      if (filterValues[key] !== null && filterValues[key] !== '' && filterValues[key] !== undefined) {
        this.appliedFilters[key as keyof VehicleFilter] = filterValues[key];
        this.filterCount++;
      }
    });

    this.currentPage = 1;
    this.loadVehicles();
    this.showFilters = false;
  }

  clearFilters(): void {
    this.filterForm.reset();
    this.appliedFilters = {};
    this.filterCount = 0;
    this.currentPage = 1;
    this.loadVehicles();
  }

  showAddEditDialog(vehicle?: Vehicle): void {
    this.resetForm();
    this.selectedPhotos = [];
    this.existingPhotos = [];
    this.photosToKeep = [];

    if (vehicle) {
      this.isEditing = true;
      this.vehicleForm.patchValue(vehicle);
      this.existingPhotos = vehicle.photoUrls || [];
      this.photosToKeep = [...this.existingPhotos];
    }
    this.displayDialog = true;
  }

  onFileSelect(event: any): void {
    const files = event.files;
    if (files && files.length > 0) {
      // Validate file types and sizes
      const validFiles = Array.from(files).filter((file: any) => {
        const isValidType = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'].includes(file.type);
        const isValidSize = file.size <= 5 * 1024 * 1024; // 5MB limit
        return isValidType && isValidSize;
      }) as File[];

      if (validFiles.length !== files.length) {
        this.messageService.add({ 
          severity: 'warn', 
          summary: 'Warning', 
          detail: 'Some files were skipped due to invalid type or size (max 5MB)' 
        });
      }

      this.selectedPhotos = [...this.selectedPhotos, ...validFiles];
    }
  }

  removeSelectedPhoto(index: number): void {
    this.selectedPhotos.splice(index, 1);
  }

  removeExistingPhoto(url: string): void {
    this.photosToKeep = this.photosToKeep.filter(photo => photo !== url);
  }

  addExistingPhoto(url: string): void {
    if (!this.photosToKeep.includes(url)) {
      this.photosToKeep.push(url);
    }
  }

  onSubmit(): void {
    if (this.vehicleForm.invalid) {
      this.markFormTouched(this.vehicleForm);
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fix the errors' });
      return;
    }

    const vehicle: Vehicle = this.vehicleForm.value;
    this.loading = true;

    if (this.isEditing) {
      this.vehicleService.updateVehicle(vehicle.vehicleId, vehicle, this.photosToKeep, this.selectedPhotos).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Vehicle updated successfully' });
          this.displayDialog = false;
          this.loadVehicles();
          this.loading = false;
        },
        error: err => {
          const errorMessage = err?.error?.Message || 'Operation failed';
          this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
          this.loading = false;
          console.error(err);
        }
      });
    } else {
      this.vehicleService.createVehicle(vehicle, this.selectedPhotos).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Vehicle created successfully' });
          this.displayDialog = false;
          this.loadVehicles();
          this.loading = false;
        },
        error: err => {
          const errorMessage = err?.error?.Message || 'Operation failed';
          this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
          this.loading = false;
          console.error(err);
        }
      });
    }
  }

  showImageGallery(vehicle: Vehicle, index: number = 0): void {
    this.selectedVehicle = vehicle;
    this.selectedImageIndex = index;
    this.displayImageDialog = true;
  }

  nextImage(): void {
    if (this.selectedVehicle && this.selectedVehicle.photoUrls) {
      this.selectedImageIndex = (this.selectedImageIndex + 1) % this.selectedVehicle.photoUrls.length;
    }
  }

  previousImage(): void {
    if (this.selectedVehicle && this.selectedVehicle.photoUrls) {
      this.selectedImageIndex = this.selectedImageIndex === 0 
        ? this.selectedVehicle.photoUrls.length - 1 
        : this.selectedImageIndex - 1;
    }
  }

  confirmDelete(vehicle: Vehicle): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete vehicle with license plate "${vehicle.licensePlate}"?`,
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteVehicle(vehicle.vehicleId)
    });
  }

  deleteVehicle(id: number): void {
    this.loading = true;
    this.vehicleService.deleteVehicle(id).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Vehicle deleted successfully' });
        this.loadVehicles();
      },
      error: err => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete vehicle' });
        this.loading = false;
        console.error(err);
      }
    });
  }

  resetForm(): void {
    this.vehicleForm.reset({ vehicleId: 0, availability: true, maxCapacity: 0 });
    this.isEditing = false;
  }

  markFormTouched(form: FormGroup): void {
    Object.values(form.controls).forEach(control => control.markAsTouched());
  }

  getFieldError(field: string): string {
    const control = this.vehicleForm.get(field);
    if (control?.touched && control?.errors) {
      if (control.errors['required']) return 'This field is required';
      if (control.errors['pattern']) return 'License plate must contain only letters, numbers, spaces, or hyphens';
      if (control.errors['min']) return `${field} must be at least 1`;
      if (control.errors['maxlength']) return 'This field must be at most 50 characters';
    }
    return '';
  }

  getVehicleTypeLabel(type: number | null | undefined): string {
    return VehicleTypeLabel[type as VehicleType] ?? 'N/A';
  }

  getImageUrl(photoUrl: string): string {
    return this.vehicleService.getImageUrl(photoUrl);
  }

  getAvailabilityBadgeClass(availability: boolean): string {
    return availability 
      ? 'bg-green-100 text-green-800 border-green-200' 
      : 'bg-red-100 text-red-800 border-red-200';
  }

  getVehicleTypeBadgeClass(type: number): string {
    switch (type) {
      case VehicleType.Commercial:
        return 'bg-blue-100 text-blue-800 border-blue-200';
      case VehicleType.Passenger:
        return 'bg-purple-100 text-purple-800 border-purple-200';
      case VehicleType.Truck:
        return 'bg-orange-100 text-orange-800 border-orange-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  }

  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'grid' ? 'table' : 'grid';
  }

  get totalPhotos(): number {
    return this.selectedPhotos.length + this.photosToKeep.length;
  }

  getPhotoPreviewUrl(file: File): string {
    return URL.createObjectURL(file);
  }
}