import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { VehicleReservation, VehicleReservationStatus, CreateVehicleReservationRequest, UpdateVehicleReservationRequest } from '../models/VehicleReservation';
import { PagedResponse } from '../models/PagedResponse';
import { environment } from '../../../environments/environment';

export interface VehicleReservationFilters {
  pageNumber?: number;
  pageSize?: number;
  requesterId?: number;
  vehicleId?: number;
  status?: VehicleReservationStatus;
  requiresDriver?: boolean;
  startDateFrom?: string;
  startDateTo?: string;
  endDateFrom?: string;
  endDateTo?: string;
  departure?: string;
  destination?: string;
}

@Injectable({
  providedIn: 'root'
})
export class VehicleReservationService {
  private apiUrl = `${environment.apiBaseUrl}/vehiclereservations`;

  constructor(private http: HttpClient) { }

  // Get all vehicle reservations
  getAllVehicleReservations(): Observable<VehicleReservation[]> {
    return this.http.get<VehicleReservation[]>(this.apiUrl);
  }

  // Get paginated vehicle reservations with filters
  getPaginatedVehicleReservations(filters: VehicleReservationFilters): Observable<PagedResponse<VehicleReservation>> {
    let params = new HttpParams();
    if (filters.pageNumber) params = params.set('pageNumber', filters.pageNumber.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
    if (filters.requesterId) params = params.set('requesterId', filters.requesterId.toString());
    if (filters.vehicleId) params = params.set('vehicleId', filters.vehicleId.toString());
    if (filters.status !== undefined) params = params.set('status', filters.status.toString());
    if (filters.requiresDriver !== undefined) params = params.set('requiresDriver', filters.requiresDriver.toString());
    if (filters.startDateFrom) params = params.set('startDateFrom', filters.startDateFrom);
    if (filters.startDateTo) params = params.set('startDateTo', filters.startDateTo);
    if (filters.endDateFrom) params = params.set('endDateFrom', filters.endDateFrom);
    if (filters.endDateTo) params = params.set('endDateTo', filters.endDateTo);
    if (filters.departure) params = params.set('departure', filters.departure);
    if (filters.destination) params = params.set('destination', filters.destination);

    return this.http.get<PagedResponse<VehicleReservation>>(`${this.apiUrl}/paged`, { params });
  }

  // Get vehicle reservation by ID
  getVehicleReservationById(id: number): Observable<VehicleReservation> {
    return this.http.get<VehicleReservation>(`${this.apiUrl}/${id}`);
  }

  // Get reservations by vehicle ID
  getReservationsByVehicleId(vehicleId: number): Observable<VehicleReservation[]> {
    return this.http.get<VehicleReservation[]>(`${this.apiUrl}/vehicle/${vehicleId}`);
  }

  // Get reservations by requester ID
  getReservationsByRequesterId(requesterId: number): Observable<VehicleReservation[]> {
    return this.http.get<VehicleReservation[]>(`${this.apiUrl}/requester/${requesterId}`);
  }

  // Get reservations by status
  getReservationsByStatus(status: VehicleReservationStatus): Observable<VehicleReservation[]> {
    return this.http.get<VehicleReservation[]>(`${this.apiUrl}/status/${status}`);
  }

  // Create vehicle reservation
  createVehicleReservation(request: CreateVehicleReservationRequest): Observable<VehicleReservation> {
    return this.http.post<VehicleReservation>(this.apiUrl, request);
  }

  // Update vehicle reservation
  updateVehicleReservation(id: number, request: UpdateVehicleReservationRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  // Delete vehicle reservation
  deleteVehicleReservation(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Approve reservation
  approveReservation(reservationId: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${reservationId}/approve`, {});
  }

  // Reject reservation
  rejectReservation(reservationId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${reservationId}/reject`, {});
  }
} 