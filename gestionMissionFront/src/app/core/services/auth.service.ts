import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { User } from '../models/User';

export interface AuthUser {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  phone: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<AuthUser | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  private isBrowser: boolean;

  constructor(
    private router: Router,
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
    // Load user immediately to prevent flash
    this.loadUserFromToken();
  }

  private loadUserFromToken(): void {
    if (!this.isBrowser) return;
    
    const token = localStorage.getItem('authToken');
    if (token) {
      try {
        const payload = this.decodeToken(token);
        if (payload && !this.isTokenExpired(payload)) {
          this.currentUserSubject.next(payload);
        } else {
          this.clearAuthData();
        }
      } catch (error) {
        console.error('Error decoding token:', error);
        this.clearAuthData();
      }
    }
  }

  private decodeToken(token: string): AuthUser | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      
      // Extract user information from JWT claims
      return {
        userId: parseInt(payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']) || 1,
        firstName: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']?.split(' ')[0] || 'John',
        lastName: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']?.split(' ')[1] || 'Doe',
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || 'john.doe@example.com',
        role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'Admin',
        phone: payload.phone || '+1234567890'
      };
    } catch (error) {
      console.error('Error decoding token:', error);
      // If token decoding fails, return null
      return null;
    }
  }

  private isTokenExpired(payload: any): boolean {
    if (!payload.exp) return false;
    const expirationDate = new Date(payload.exp * 1000);
    return expirationDate < new Date();
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'accept': '*/*'
    });

    return this.http.post<LoginResponse>(`${environment.apiBaseUrl}/Users/login`, credentials, { headers })
      .pipe(
        tap(response => {
          if (this.isBrowser && response.token) {
            localStorage.setItem('authToken', response.token);
            // Decode user info from token and set current user
            const user = this.decodeToken(response.token);
            if (user) {
              this.currentUserSubject.next(user);
            }
          }
        }),
        catchError(error => {
          console.error('Login error:', error);
          return throwError(() => error);
        })
      );
  }

  logout(): void {
    // Clear all authentication data
    this.clearAuthData();
    // Navigate to login
    this.router.navigate(['/login']);
  }

  private clearAuthData(): void {
    if (this.isBrowser) {
      // Remove token from localStorage
      localStorage.removeItem('authToken');
      // Clear any other auth-related data
      localStorage.removeItem('user');
      sessionStorage.removeItem('authToken');
      sessionStorage.removeItem('user');
    }
    // Clear current user from memory
    this.currentUserSubject.next(null);
  }

  // Method to force clear all auth data (useful for security)
  forceLogout(): void {
    this.clearAuthData();
    this.router.navigate(['/login']);
  }

  getCurrentUser(): AuthUser | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    // Check both the BehaviorSubject and localStorage for immediate response
    if (this.currentUserSubject.value !== null) {
      return true;
    }
    
    // If BehaviorSubject is null but token exists, try to load it
    if (this.isBrowser) {
      const token = localStorage.getItem('authToken');
      if (token) {
        try {
          const payload = this.decodeToken(token);
          if (payload && !this.isTokenExpired(payload)) {
            this.currentUserSubject.next(payload);
            return true;
          }
        } catch (error) {
          // Token is invalid, clear it
          this.clearAuthData();
        }
      }
    }
    
    return false;
  }

  getToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem('authToken');
  }
} 