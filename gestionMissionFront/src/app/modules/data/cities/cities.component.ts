import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { City } from '../../../core/models/City';
import { PagedResponse } from '../../../core/models/PagedResponse';
import { CityService } from '../../../core/services/city.service';
import { SharedModule } from '../../../shared/shared.module';

@Component({
  selector: 'app-cities',
  standalone: true,
  templateUrl: './cities.component.html',
  styleUrls: ['./cities.component.scss'],
  imports: [SharedModule],
  providers: [MessageService, ConfirmationService, CityService]
})
export class CitiesComponent implements OnInit {
  cities: City[] = [];
  totalRecords = 0;
  loading = false;
  displayDialog = false;
  cityForm: FormGroup;
  isEditing = false;
  currentPage: number = 1;
  pageSize: number = 10;

  constructor(
    private fb: FormBuilder,
    private cityService: CityService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {
    this.cityForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadCities();
  }

  createForm(): FormGroup {
    return this.fb.group({
      cityId: [0],
      name: ['', [Validators.required, Validators.minLength(2)]],
      postalCode: ['', [Validators.required]],
      region: ['', [Validators.required]]
    });
  }

  loadCities(): void {
    this.loading = true;
    this.cityService.getPagedCities(this.currentPage, this.pageSize).subscribe({
      next: (response: PagedResponse<City>) => {
        this.cities = response.data;
        this.totalRecords = response.totalRecords;
        this.loading = false;
      },
      error: err => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load cities' });
        this.loading = false;
        console.error(err);
      }
    });
  }

  onPageChange(event: any): void {
    this.currentPage = event.page + 1;
    this.pageSize = event.rows;
    this.loadCities();
  }

  showAddEditDialog(city?: City): void {
    this.resetForm();
    if (city) {
      this.isEditing = true;
      this.cityForm.patchValue(city);
    }
    this.displayDialog = true;
  }

  onSubmit(): void {
    if (this.cityForm.invalid) {
      this.markFormTouched(this.cityForm);
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fix the errors' });
      return;
    }

    const city: City = this.cityForm.value;
    this.loading = true;

    if (this.isEditing) {
      this.cityService.updateCity(city.cityId, city).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: `City updated successfully` });
          this.displayDialog = false;
          this.loadCities();
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
      this.cityService.createCity(city).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: `City created successfully` });
          this.displayDialog = false;
          this.loadCities();
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

  confirmDelete(city: City): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete city "${city.name}"?`,
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteCity(city.cityId)
    });
  }

  deleteCity(id: number): void {
    this.loading = true;
    this.cityService.deleteCity(id).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'City deleted successfully' });
        this.loadCities();
      },
      error: err => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete city' });
        this.loading = false;
        console.error(err);
      }
    });
  }

  resetForm(): void {
    this.cityForm.reset({ cityId: 0 });
    this.isEditing = false;
  }

  markFormTouched(form: FormGroup): void {
    Object.values(form.controls).forEach(control => control.markAsTouched());
  }

  getFieldError(field: string): string {
    const control = this.cityForm.get(field);
    if (control?.touched && control?.errors) {
      if (control.errors['required']) return 'This field is required';
      if (control.errors['minlength']) return `Min length is ${control.errors['minlength'].requiredLength}`;
    }
    return '';
  }
}
