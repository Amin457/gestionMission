import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Vehicle, VehicleFilter } from '../models/Vehicle';
import { PagedResponse } from '../models/PagedResponse';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class VehicleService {
  private apiUrl = `${environment.apiBaseUrl}/Vehicles`;

  constructor(private http: HttpClient) {}

  getPagedVehicles(pageNumber: number, pageSize: number, filter?: VehicleFilter): Observable<PagedResponse<Vehicle>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (filter) {
      if (filter.type !== undefined) params = params.set('filter.Type', filter.type.toString());
      if (filter.availability !== undefined) params = params.set('filter.Availability', filter.availability.toString());
      if (filter.licensePlate) params = params.set('filter.LicensePlate', filter.licensePlate);
      if (filter.minCapacity !== undefined) params = params.set('filter.MinCapacity', filter.minCapacity.toString());
      if (filter.maxCapacity !== undefined) params = params.set('filter.MaxCapacity', filter.maxCapacity.toString());
    }

    return this.http.get<PagedResponse<Vehicle>>(`${this.apiUrl}/paged`, { params });
  }

  getAllVehicles(): Observable<Vehicle[]> {
    return this.http.get<Vehicle[]>(this.apiUrl);
  }

  getAvailableVehicles(): Observable<Vehicle[]> {
    return this.http.get<Vehicle[]>(`${this.apiUrl}/available`);
  }

  getVehicle(id: number): Observable<Vehicle> {
    return this.http.get<Vehicle>(`${this.apiUrl}/${id}`);
  }

  createVehicle(vehicle: Vehicle, photos?: File[]): Observable<Vehicle> {
    const formData = new FormData();
    
    // Add vehicle data
    formData.append('Type', vehicle.type.toString());
    formData.append('LicensePlate', vehicle.licensePlate);
    formData.append('Availability', vehicle.availability.toString());
    formData.append('MaxCapacity', vehicle.maxCapacity.toString());

    // Add photos if provided
    if (photos && photos.length > 0) {
      photos.forEach(photo => {
        formData.append('Photos', photo);
      });
    }

    return this.http.post<Vehicle>(this.apiUrl, formData);
  }

  updateVehicle(id: number, vehicle: Vehicle, keepPhotosUrls?: string[], newPhotos?: File[]): Observable<void> {
    const formData = new FormData();
    
    // Add vehicle data
    formData.append('VehicleId', vehicle.vehicleId.toString());
    formData.append('Type', vehicle.type.toString());
    formData.append('LicensePlate', vehicle.licensePlate);
    formData.append('Availability', vehicle.availability.toString());
    formData.append('MaxCapacity', vehicle.maxCapacity.toString());

    // Add existing photos to keep
    if (keepPhotosUrls && keepPhotosUrls.length > 0) {
      keepPhotosUrls.forEach(url => {
        formData.append('KeepPhotosUrls', url);
      });
    }

    // Add new photos
    if (newPhotos && newPhotos.length > 0) {
      newPhotos.forEach(photo => {
        formData.append('NewPhotos', photo);
      });
    }

    return this.http.put<void>(`${this.apiUrl}/${id}`, formData);
  }

  deleteVehicle(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getVehicleCountByType(type: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/count/type/${type}`);
  }

  getImageUrl(photoUrl: string): string {
    if (!photoUrl) return '';
    if (photoUrl.startsWith('http')) return photoUrl;
    // Remove 'api' from the base URL for image paths
    const baseUrl = environment.apiBaseUrl.replace('/api', '');
    return `${baseUrl}/${photoUrl}`;
  }
}