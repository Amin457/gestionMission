import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResponse } from '../models/PagedResponse';
import { City } from '../models/City';


@Injectable({
  providedIn: 'root'
})
export class CityService {
  private baseUrl = `${environment.apiBaseUrl}/Cities`;

  constructor(private http: HttpClient) {}

  // 🔹 Get all cities
  getAll(): Observable<City[]> {
    return this.http.get<City[]>(this.baseUrl);
  }

  // 🔹 Get city by ID
  getById(id: number): Observable<City> {
    return this.http.get<City>(`${this.baseUrl}/${id}`);
  }

  // 🔹 Get paginated list
  getPagedCities(pageNumber: number, pageSize: number): Observable<PagedResponse<City>> {
    const params = new HttpParams()
    .set('pageNumber', pageNumber.toString())
    .set('pageSize', pageSize.toString());
    return this.http.get<PagedResponse<City>>(`${this.baseUrl}/paged`, { params });
  }

  // 🔹 Create new city
  createCity(city: City): Observable<City> {
    return this.http.post<City>(this.baseUrl, city);
  }

  // 🔹 Update city
  updateCity(id: number, city: City): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, city);
  }

  // 🔹 Delete city
  deleteCity(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
