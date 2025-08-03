import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Circuit } from '../models/Circuit';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CircuitService {
  private baseUrl = `${environment.apiBaseUrl}`;

  constructor(private http: HttpClient) { }

  getCircuitsForMission(missionId: number): Observable<Circuit[]> {
    return this.http.get<Circuit[]>(`${this.baseUrl}/Circuits/mission/${missionId}`);
  }

  generateCircuitsForMission(missionId: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/missions/${missionId}/generate-circuits`, {});
  }
} 