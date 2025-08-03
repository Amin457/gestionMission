import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MissionCost, MissionCostType } from '../models/MissionCost';
import { PagedResponse } from '../models/PagedResponse';

export interface MissionCostFilters {
  pageNumber?: number;
  pageSize?: number;
  missionId?: number;
  type?: MissionCostType;
  minAmount?: number;
  maxAmount?: number;
  dateStart?: string;
  dateEnd?: string;
}

@Injectable({
  providedIn: 'root'
})
export class MissionCostService {
  private apiUrl = `${environment.apiBaseUrl}/missioncosts`;

  constructor(private http: HttpClient) { }

  // Get all mission costs
  getAllMissionCosts(): Observable<MissionCost[]> {
    return this.http.get<MissionCost[]>(this.apiUrl);
  }

  // Get mission cost by ID
  getMissionCostById(id: number): Observable<MissionCost> {
    return this.http.get<MissionCost>(`${this.apiUrl}/${id}`);
  }

  // Get mission costs by mission ID
  getMissionCostsByMissionId(missionId: number): Observable<MissionCost[]> {
    return this.http.get<MissionCost[]>(`${this.apiUrl}/mission/${missionId}`);
  }

  // Get total cost by mission ID
  getTotalCostByMissionId(missionId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/total/mission/${missionId}`);
  }

  // Get paginated mission costs with filters
  getPaginatedMissionCosts(filters: MissionCostFilters): Observable<PagedResponse<MissionCost>> {
    let params = new HttpParams();
    if (filters.pageNumber) params = params.set('pageNumber', filters.pageNumber.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
    if (filters.missionId) params = params.set('missionId', filters.missionId.toString());
    if (filters.type) params = params.set('type', filters.type.toString());
    if (filters.minAmount) params = params.set('minAmount', filters.minAmount.toString());
    if (filters.maxAmount) params = params.set('maxAmount', filters.maxAmount.toString());
    if (filters.dateStart) params = params.set('dateStart', filters.dateStart);
    if (filters.dateEnd) params = params.set('dateEnd', filters.dateEnd);

    return this.http.get<PagedResponse<MissionCost>>(`${this.apiUrl}/paged`, { params });
  }

  // Get image URL for receipt photos
  getImageUrl(photoUrl: string): string {
    if (!photoUrl) return '';
    if (photoUrl.startsWith('http')) return photoUrl;
    // Remove 'api' from the base URL for image paths
    const baseUrl = environment.apiBaseUrl.replace('/api', '');
    return `${baseUrl}/${photoUrl}`;
  }

  // Create mission cost with photos (multipart/form-data)
  createMissionCost(
    missionId: number,
    type: MissionCostType,
    amount: number,
    date: string,
    receiptPhotos: File[] = []
  ): Observable<MissionCost> {
    const formData = new FormData();
    formData.append('missionId', missionId.toString());
    formData.append('type', type.toString());
    formData.append('amount', amount.toString());
    formData.append('date', date);
    
    receiptPhotos.forEach((photo, index) => {
      formData.append('receiptPhotos', photo);
    });

    return this.http.post<MissionCost>(this.apiUrl, formData);
  }

  // Update mission cost with photos (multipart/form-data)
  updateMissionCost(
    id: number,
    missionId: number,
    type: MissionCostType,
    amount: number,
    date: string,
    receiptPhotos: File[] = []
  ): Observable<void> {
    const formData = new FormData();
    formData.append('missionId', missionId.toString());
    formData.append('type', type.toString());
    formData.append('amount', amount.toString());
    formData.append('date', date);
    
    receiptPhotos.forEach((photo, index) => {
      formData.append('receiptPhotos', photo);
    });

    return this.http.put<void>(`${this.apiUrl}/${id}`, formData);
  }

  // Delete mission cost
  deleteMissionCost(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
} 