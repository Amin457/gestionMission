import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class SidebarService {
  private _isCollapsed = new BehaviorSubject<boolean>(false);
  public isCollapsed$ = this._isCollapsed.asObservable();
  private isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {
    this.isBrowser = isPlatformBrowser(this.platformId);
    
    // Only access localStorage in browser environment
    if (this.isBrowser) {
      const savedState = localStorage.getItem('sidebarCollapsed');
      if (savedState !== null) {
        this._isCollapsed.next(savedState === 'true');
      }
    }
  }

  toggleSidebar() {
    const newState = !this._isCollapsed.value;
    this._isCollapsed.next(newState);
    
    // Only save to localStorage in browser environment
    if (this.isBrowser) {
      localStorage.setItem('sidebarCollapsed', String(newState));
    }
  }

  collapseSidebar() {
    this._isCollapsed.next(true);
    
    if (this.isBrowser) {
      localStorage.setItem('sidebarCollapsed', 'true');
    }
  }

  expandSidebar() {
    this._isCollapsed.next(false);
    
    if (this.isBrowser) {
      localStorage.setItem('sidebarCollapsed', 'false');
    }
  }
}