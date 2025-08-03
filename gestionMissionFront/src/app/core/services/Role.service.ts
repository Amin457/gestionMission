import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Role } from '../models/Role';
import { environment } from '../../../environments/environment';
import { PagedResponse } from '../models/PagedResponse';

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private apiUrl = `${environment.apiBaseUrl}/Role`; // Use the environment variable

  constructor(private http: HttpClient) {}

  // GET: Get all roles
  getAllRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(this.apiUrl);
  }

  getPagedRoles(pageNumber: number, pageSize: number): Observable<PagedResponse<Role>> {
      const params = new HttpParams()
        .set('pageNumber', pageNumber.toString())
        .set('pageSize', pageSize.toString());
      
      return this.http.get<PagedResponse<Role>>(`${this.apiUrl}/paged`, { params });
    }
  // GET: Get role by ID
  getRoleById(id: number): Observable<Role> {
    return this.http.get<Role>(`${this.apiUrl}/${id}`);
  }

  // GET: Get role by name
  getRoleByName(name: string): Observable<Role> {
    return this.http.get<Role>(`${this.apiUrl}/name/${name}`);
  }

  // POST: Create a new role
  createRole(role: Role): Observable<Role> {
    return this.http.post<Role>(this.apiUrl, role);
  }

  // PUT: Update an existing role
  updateRole(id: number, role: Role): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, role);
  }

  // DELETE: Delete a role
  deleteRole(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
