import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Mission, MissionGet } from '../models/Mission';

interface MissionPagedResponse {
  data: MissionGet[];
  totalRecords: number;
}

@Injectable({
  providedIn: 'root'
})
export class MissionService {
  private baseUrl = `${environment.apiBaseUrl}/missions`;

  constructor(private http: HttpClient) {}

  getMissionsPaged(pageNumber: number, pageSize: number, filters: any): Observable<MissionPagedResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
      if (filters) {
        if (filters.type != null) params = params.set('type', filters.type.toString());
        if (filters.status != null) params = params.set('status', filters.status.toString());
        if (filters.driverId != null) params = params.set('driverId', filters.driverId.toString());
        if (filters.quantity != null) params = params.set('quantity', filters.quantity.toString());
        if (filters.desiredDateStart) params = params.set('desiredDateStart', filters.desiredDateStart.toISOString());
        if (filters.desiredDateEnd) params = params.set('desiredDateEnd', filters.desiredDateEnd.toISOString());
      }
      console.log(params);
    return this.http.get<MissionPagedResponse>(`${this.baseUrl}/paged`, { params });
  }


  getMissionById(id: number): Observable<Mission> {
    return this.http.get<Mission>(`${this.baseUrl}/${id}`);
  }

  createMission(mission: Mission): Observable<Mission> {
    return this.http.post<Mission>(this.baseUrl, mission);
  }

  updateMission(id: number, mission: Mission): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, mission);
  }

  deleteMission(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
