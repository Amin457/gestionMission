import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Trip {
  id: number;
  driverId: number;
  driverName: string;
  startDate: Date;
  endDate: Date;
  status: string;
  // Add more properties as needed
}

export interface PagedTripResponse {
  data: Trip[];
  totalRecords: number;
}

@Injectable({
  providedIn: 'root'
})
export class TripService {
  private apiUrl = `${environment.apiBaseUrl}/Trips`;

  constructor(private http: HttpClient) {}

  getPagedTrips(pageNumber: number, pageSize: number, filters: any): Observable<PagedTripResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (filters) {
      if (filters.status) params = params.set('status', filters.status);
      if (filters.driverId) params = params.set('driverId', filters.driverId.toString());
      if (filters.dateRange) {
        if (filters.dateRange[0]) params = params.set('startDate', filters.dateRange[0].toISOString());
        if (filters.dateRange[1]) params = params.set('endDate', filters.dateRange[1].toISOString());
      }
    }

    return this.http.get<PagedTripResponse>(`${this.apiUrl}/paged`, { params });
  }

  getTripById(id: number): Observable<Trip> {
    return this.http.get<Trip>(`${this.apiUrl}/${id}`);
  }

  generateTrip(): Observable<any> {
    return this.http.post(`${this.apiUrl}/generate`, {});
  }
} 