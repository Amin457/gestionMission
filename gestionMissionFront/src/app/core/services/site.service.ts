import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Site } from '../models/Site';
import { PagedResponse } from '../models/PagedResponse';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SiteService {
  private apiUrl = `${environment.apiBaseUrl}/Sites`;

  constructor(private http: HttpClient) {}

  getPagedSites(pageNumber: number, pageSize: number): Observable<PagedResponse<Site>> {
      const params = new HttpParams()
        .set('pageNumber', pageNumber.toString())
        .set('pageSize', pageSize.toString());
      
      return this.http.get<PagedResponse<Site>>(`${this.apiUrl}/paged`, { params });  }

  getSite(id: number): Observable<Site> {
    return this.http.get<Site>(`${this.apiUrl}/${id}`);
  }

  createSite(site: Site): Observable<Site> {
    return this.http.post<Site>(this.apiUrl, site);
  }

  updateSite(id: number, site: Site): Observable<Site> {
    return this.http.put<Site>(`${this.apiUrl}/${id}`, site);
  }

  deleteSite(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}