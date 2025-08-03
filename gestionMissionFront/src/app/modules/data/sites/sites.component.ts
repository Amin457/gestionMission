import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { Site } from '../../../core/models/Site';
import { City } from '../../../core/models/City';
import { PagedResponse } from '../../../core/models/PagedResponse';
import { SiteService } from '../../../core/services/site.service';
import { CityService } from '../../../core/services/city.service';
import { SharedModule } from '../../../shared/shared.module';
import { MapPickerComponent } from '../../../shared/components/map-picker/map-picker.component';

@Component({
  selector: 'app-sites',
  standalone: true,
  templateUrl: './sites.component.html',
  styleUrls: ['./sites.component.scss'],
  imports: [CommonModule, SharedModule, MapPickerComponent, ReactiveFormsModule],
  providers: [MessageService, ConfirmationService, SiteService, CityService]
})
export class SitesComponent implements OnInit {
  sites: Site[] = [];
  cities: City[] = [];
  totalRecords = 0;
  loading = false;
  displayDialog = false;
  siteForm: FormGroup;
  isEditing = false;
  currentPage: number = 1;
  pageSize: number = 10;

  constructor(
    private fb: FormBuilder,
    private siteService: SiteService,
    private cityService: CityService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {
    this.siteForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadSites();
    this.loadCities();
  }

  createForm(): FormGroup {
    return this.fb.group({
      siteId: [0],
      name: ['', [Validators.required, Validators.minLength(2)]],
      type: ['', [Validators.required]],
      address: ['', [Validators.required]],
      phone: ['', [Validators.required, Validators.pattern('^[0-9]{8}$')]],
      cityId: [null, [Validators.required]],
      latitude: [0, [Validators.required, Validators.min(-90), Validators.max(90)]],
      longitude: [0, [Validators.required, Validators.min(-180), Validators.max(180)]]
    });
  }

  loadSites(): void {
    this.loading = true;
    this.siteService.getPagedSites(this.currentPage, this.pageSize).subscribe({
      next: (response: PagedResponse<Site>) => {
        this.sites = response.data;
        this.totalRecords = response.totalRecords;
        this.loading = false;
      },
      error: err => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load sites' });
        this.loading = false;
        console.error(err);
      }
    });
  }

  loadCities(): void {
    this.cityService.getAll().subscribe({
      next: (cities: City[]) => {
        this.cities = cities;
      },
      error: err => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load cities' });
        console.error(err);
      }
    });
  }

  onPageChange(event: any): void {
    this.currentPage = event.page + 1;
    this.pageSize = event.rows;
    this.loadSites();
  }

  showAddEditDialog(site?: Site): void {
    this.resetForm();
    if (site) {
      this.isEditing = true;
      this.siteForm.patchValue({
        ...site,
        latitude: site.latitude || 36.8065,
        longitude: site.longitude || 10.1815
      });
    } else {
      this.siteForm.patchValue({
        siteId: 0,
        cityId: null,
        latitude: 36.8065,
        longitude: 10.1815
      });
    }
    this.displayDialog = true;
  }

  onSubmit(): void {
    if (this.siteForm.invalid) {
      this.markFormTouched(this.siteForm);
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fix the errors' });
      return;
    }

    const site: Site = this.siteForm.value;
    this.loading = true;

    if (this.isEditing) {
      this.siteService.updateSite(site.siteId, site).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Site updated successfully' });
          this.displayDialog = false;
          this.loadSites();
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
      this.siteService.createSite(site).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Site created successfully' });
          this.displayDialog = false;
          this.loadSites();
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

  confirmDelete(site: Site): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete site "${site.name}"?`,
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteSite(site.siteId)
    });
  }

  deleteSite(id: number): void {
    this.loading = true;
    this.siteService.deleteSite(id).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Site deleted successfully' });
        this.loadSites();
      },
      error: err => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete site' });
        this.loading = false;
        console.error(err);
      }
    });
  }

  resetForm(): void {
    this.siteForm.reset({ siteId: 0, cityId: null });
    this.isEditing = false;
  }

  markFormTouched(form: FormGroup): void {
    Object.values(form.controls).forEach(control => control.markAsTouched());
  }

  getFieldError(field: string): string {
    const control = this.siteForm.get(field);
    if (control?.touched && control?.errors) {
      if (control.errors['required']) return 'This field is required';
      if (control.errors['minlength']) return `Min length is ${control.errors['minlength'].requiredLength}`;
      if (control.errors['pattern']) return 'Phone must be a 8-digit number';
      if (control.errors['min']) return `Value must be at least ${control.errors['min'].min}`;
      if (control.errors['max']) return `Value must be at most ${control.errors['max'].max}`;
    }
    return '';
  }

  onCoordinatesChange(coords: {latitude: number, longitude: number}) {
    this.siteForm.patchValue({
      latitude: coords.latitude,
      longitude: coords.longitude
    });
  }
}