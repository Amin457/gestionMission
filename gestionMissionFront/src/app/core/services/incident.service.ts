import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Incident, IncidentType, IncidentStatus } from '../models/Incident';
import { PagedResponse } from '../models/PagedResponse';

export interface IncidentFilters {
  pageNumber?: number;
  pageSize?: number;
  missionId?: number;
  type?: IncidentType;
  status?: IncidentStatus;
  reportDateStart?: string;
  reportDateEnd?: string;
}

@Injectable({
  providedIn: 'root'
})
export class IncidentService {
  private apiUrl = `${environment.apiBaseUrl}/incidents`;

  constructor(private http: HttpClient) { }

  // Get all incidents
  getAllIncidents(): Observable<Incident[]> {
    return this.http.get<Incident[]>(this.apiUrl);
  }

  // Get paginated incidents with filters
  getPaginatedIncidents(filters: IncidentFilters): Observable<PagedResponse<Incident>> {
    let params = new HttpParams();
    if (filters.pageNumber) params = params.set('pageNumber', filters.pageNumber.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
    if (filters.missionId) params = params.set('missionId', filters.missionId.toString());
    if (filters.type) params = params.set('type', filters.type.toString());
    if (filters.status) params = params.set('status', filters.status.toString());
    if (filters.reportDateStart) params = params.set('reportDateStart', filters.reportDateStart);
    if (filters.reportDateEnd) params = params.set('reportDateEnd', filters.reportDateEnd);

    return this.http.get<PagedResponse<Incident>>(`${this.apiUrl}/paged`, { params });
  }

  // Get incident by ID
  getIncidentById(id: number): Observable<Incident> {
    return this.http.get<Incident>(`${this.apiUrl}/${id}`);
  }

  // Get incidents by mission ID
  getIncidentsByMissionId(missionId: number): Observable<Incident[]> {
    return this.http.get<Incident[]>(`${this.apiUrl}/mission/${missionId}`);
  }

  // Get incident count by mission ID
  getIncidentCountByMissionId(missionId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/count/mission/${missionId}`);
  }

  // Get image/document URL for incident files
  getFileUrl(fileUrl: string): string {
    if (!fileUrl) return '';
    if (fileUrl.startsWith('http')) return fileUrl;
    // Remove 'api' from the base URL for file paths
    const baseUrl = environment.apiBaseUrl.replace('/api', '');
    return `${baseUrl}/${fileUrl}`;
  }

  // Create incident with documents/images (multipart/form-data)
  createIncident(
    missionId: number,
    type: IncidentType,
    description: string,
    reportDate: string,
    status: IncidentStatus,
    incidentDocs: File[] = []
  ): Observable<Incident> {
    const formData = new FormData();
    formData.append('missionId', missionId.toString());
    formData.append('type', type.toString());
    formData.append('description', description);
    formData.append('reportDate', reportDate);
    formData.append('status', status.toString());
    
    incidentDocs.forEach((file, index) => {
      formData.append('incidentDocs', file);
    });

    return this.http.post<Incident>(this.apiUrl, formData);
  }

  // Update incident with documents/images (multipart/form-data)
  updateIncident(
    id: number,
    missionId: number,
    type: IncidentType,
    description: string,
    reportDate: string,
    status: IncidentStatus,
    incidentDocs: File[] = []
  ): Observable<void> {
    const formData = new FormData();
    formData.append('missionId', missionId.toString());
    formData.append('type', type.toString());
    formData.append('description', description);
    formData.append('reportDate', reportDate);
    formData.append('status', status.toString());
    
    incidentDocs.forEach((file, index) => {
      formData.append('incidentDocs', file);
    });

    return this.http.put<void>(`${this.apiUrl}/${id}`, formData);
  }

  // Delete incident
  deleteIncident(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
} 