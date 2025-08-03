import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  NotificationDto,
  CreateNotificationDto,
  NotificationFilter,
  NotificationStatsDto,
  UpdateNotificationStatusDto,
  RealTimeNotificationDto,
  NotificationStatus,
  NotificationCategory,
  NotificationPriority
} from '../models/notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly baseUrl = `${environment.apiBaseUrl}/notification`;
  
  private notificationsSubject = new BehaviorSubject<NotificationDto[]>([]);
  private statsSubject = new BehaviorSubject<NotificationStatsDto>({
    total: 0,
    unread: 0,
    read: 0,
    archived: 0
  });

  public notifications$ = this.notificationsSubject.asObservable();
  public stats$ = this.statsSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * Get user notifications with optional filtering
   */
  getNotifications(filter?: NotificationFilter): Observable<NotificationDto[]> {
    let params = new HttpParams();
    
    if (filter) {
      if (filter.status) params = params.set('status', filter.status);
      if (filter.type) params = params.set('type', filter.type);
      if (filter.priority) params = params.set('priority', filter.priority);
      if (filter.dateFrom) params = params.set('dateFrom', filter.dateFrom);
      if (filter.dateTo) params = params.set('dateTo', filter.dateTo);
      if (filter.relatedEntityType) params = params.set('relatedEntityType', filter.relatedEntityType);
      if (filter.relatedEntityId) params = params.set('relatedEntityId', filter.relatedEntityId.toString());
    }

    return this.http.get<NotificationDto[]>(this.baseUrl, { params });
  }

  /**
   * Get notification by ID
   */
  getNotificationById(id: number): Observable<NotificationDto> {
    return this.http.get<NotificationDto>(`${this.baseUrl}/${id}`);
  }

  /**
   * Create a new notification
   */
  createNotification(notification: CreateNotificationDto): Observable<NotificationDto> {
    return this.http.post<NotificationDto>(this.baseUrl, notification);
  }

  /**
   * Update notification status
   */
  updateNotificationStatus(id: number, status: NotificationStatus): Observable<void> {
    // Convert enum to integer value for backend API
    const statusValue = this.getStatusIntegerValue(status);
    return this.http.patch<void>(`${this.baseUrl}/${id}/status?Status=${statusValue}`, {});
  }

  /**
   * Get integer value for notification status enum
   */
  private getStatusIntegerValue(status: NotificationStatus): number {
    switch (status) {
      case NotificationStatus.Unread: return 0;
      case NotificationStatus.Read: return 1;
      case NotificationStatus.Archived: return 2;
      default: return 0;
    }
  }

  /**
   * Delete notification
   */
  deleteNotification(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /**
   * Get notification statistics
   */
  getNotificationStats(): Observable<NotificationStatsDto> {
    return this.http.get<NotificationStatsDto>(`${this.baseUrl}/stats`);
  }

  /**
   * Mark all notifications as read
   */
  markAllAsRead(): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/mark-all-read`, {}).pipe(
      tap(() => {
        // Update local state after successful API call
        const currentNotifications = this.notificationsSubject.value;
        const updatedNotifications = currentNotifications.map(notification => ({
          ...notification,
          status: NotificationStatus.Read
        }));
        this.notificationsSubject.next(updatedNotifications);
        
        // Update stats
        const currentStats = this.statsSubject.value;
        const unreadCount = currentNotifications.filter(n => n.status === NotificationStatus.Unread).length;
        const updatedStats = {
          ...currentStats,
          unread: 0,
          read: currentStats.read + unreadCount
        };
        this.statsSubject.next(updatedStats);
      })
    );
  }

  /**
   * Send real-time notification
   */
  sendRealTimeNotification(notification: RealTimeNotificationDto): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/send-realtime`, notification);
  }

  /**
   * Transform raw notification data to match NotificationDto interface
   */
  private transformNotificationData(rawData: any): NotificationDto {
    console.log('API Raw notification data:', rawData);
    console.log('API Priority value:', rawData.priority, 'Type:', typeof rawData.priority);
    console.log('API Status value:', rawData.status, 'Type:', typeof rawData.status);
    console.log('API Type value:', rawData.notificationType, 'Type:', typeof rawData.notificationType);
    
    return {
      notificationId: rawData.notificationId || 0,
      userId: rawData.userId || 0,
      title: rawData.title || 'Notification',
      message: rawData.message || '',
      sentDate: rawData.sentDate || new Date().toISOString(),
      notificationType: this.mapNotificationType(rawData.notificationType),
      priority: this.mapNotificationPriority(rawData.priority),
      status: this.mapNotificationStatus(rawData.status),
      relatedEntityType: rawData.relatedEntityType,
      relatedEntityId: rawData.relatedEntityId,
      expiryDate: rawData.expiryDate
    };
  }

  /**
   * Map notification type to enum (handles both string and numeric values)
   */
  private mapNotificationType(type: any): NotificationCategory {
    if (typeof type === 'string') {
      // Handle string values from backend
      switch (type.toLowerCase()) {
        case 'mission': return NotificationCategory.Mission;
        case 'task': return NotificationCategory.Task;
        case 'reservation': return NotificationCategory.Reservation;
        case 'incident': return NotificationCategory.Incident;
        case 'system': return NotificationCategory.System;
        case 'alert': return NotificationCategory.Alert;
        default: return NotificationCategory.System;
      }
    }
    
    // Handle numeric values
    switch (Number(type)) {
      case 0: return NotificationCategory.Mission;
      case 1: return NotificationCategory.Task;
      case 2: return NotificationCategory.Reservation;
      case 3: return NotificationCategory.Incident;
      case 4: return NotificationCategory.System;
      case 5: return NotificationCategory.Alert;
      default: return NotificationCategory.System;
    }
  }

  /**
   * Map priority to enum (handles both string and numeric values)
   */
  private mapNotificationPriority(priority: any): NotificationPriority {
    if (typeof priority === 'string') {
      // Handle string values from backend
      switch (priority.toLowerCase()) {
        case 'low': return NotificationPriority.Low;
        case 'normal': return NotificationPriority.Normal;
        case 'high': return NotificationPriority.High;
        case 'urgent': return NotificationPriority.Urgent;
        default: return NotificationPriority.Normal;
      }
    }
    
    // Handle numeric values
    switch (Number(priority)) {
      case 0: return NotificationPriority.Low;
      case 1: return NotificationPriority.Normal;
      case 2: return NotificationPriority.High;
      case 3: return NotificationPriority.Urgent;
      default: return NotificationPriority.Normal;
    }
  }

  /**
   * Map status to enum (handles both string and numeric values)
   */
  private mapNotificationStatus(status: any): NotificationStatus {
    if (typeof status === 'string') {
      // Handle string values from backend
      switch (status.toLowerCase()) {
        case 'unread': return NotificationStatus.Unread;
        case 'read': return NotificationStatus.Read;
        case 'archived': return NotificationStatus.Archived;
        default: return NotificationStatus.Unread;
      }
    }
    
    // Handle numeric values
    switch (Number(status)) {
      case 0: return NotificationStatus.Unread;
      case 1: return NotificationStatus.Read;
      case 2: return NotificationStatus.Archived;
      default: return NotificationStatus.Unread;
    }
  }

  /**
   * Load notifications and update local state
   */
  loadNotifications(filter?: NotificationFilter): void {
    this.getNotifications(filter).subscribe({
      next: (notifications) => {
        console.log('Loaded notifications:', notifications);
        // Transform the data to ensure it matches the interface
        const transformedNotifications = notifications.map(notification => 
          this.transformNotificationData(notification)
        );
        this.notificationsSubject.next(transformedNotifications);
      },
      error: (error) => {
        console.error('Error loading notifications:', error);
        // Handle specific error cases
        if (error.status === 401) {
          console.error('Authentication failed - user may need to login again');
        } else if (error.status === 404) {
          console.error('Notification endpoint not found - check API configuration');
        } else if (error.status === 500) {
          console.error('Server error - notification service may be down');
          console.error('Backend needs to implement notification endpoints');
        }
        // Set empty array to prevent UI errors
        this.notificationsSubject.next([]);
      }
    });
  }

  /**
   * Load notification statistics and update local state
   */
  loadNotificationStats(): void {
    this.getNotificationStats().subscribe({
      next: (stats) => {
        this.statsSubject.next(stats);
      },
      error: (error) => {
        console.error('Error loading notification stats:', error);
        // Set default stats when API is not available
        this.statsSubject.next({
          total: 0,
          unread: 0,
          read: 0,
          archived: 0
        });
      }
    });
  }

  /**
   * Add notification to local state (for real-time updates)
   */
  addNotification(notification: NotificationDto): void {
    // Transform the notification to ensure it has all required properties
    const transformedNotification = this.transformNotificationData(notification);
    
    const currentNotifications = this.notificationsSubject.value;
    const updatedNotifications = [transformedNotification, ...currentNotifications];
    this.notificationsSubject.next(updatedNotifications);
    
    // Update stats
    const currentStats = this.statsSubject.value;
    const updatedStats = {
      ...currentStats,
      total: currentStats.total + 1,
      unread: currentStats.unread + 1
    };
    this.statsSubject.next(updatedStats);
  }

  /**
   * Update notification in local state
   */
  updateNotification(notificationId: number, updates: Partial<NotificationDto>): void {
    const currentNotifications = this.notificationsSubject.value;
    const updatedNotifications = currentNotifications.map(notification => 
      notification.notificationId === notificationId 
        ? { ...notification, ...updates }
        : notification
    );
    this.notificationsSubject.next(updatedNotifications);
  }

  /**
   * Remove notification from local state
   */
  removeNotification(notificationId: number): void {
    const currentNotifications = this.notificationsSubject.value;
    const updatedNotifications = currentNotifications.filter(
      notification => notification.notificationId !== notificationId
    );
    this.notificationsSubject.next(updatedNotifications);
    
    // Update stats
    const currentStats = this.statsSubject.value;
    const updatedStats = {
      ...currentStats,
      total: currentStats.total - 1
    };
    this.statsSubject.next(updatedStats);
  }

  /**
   * Mark notification as read
   */
  markAsRead(notificationId: number): void {
    this.updateNotificationStatus(notificationId, NotificationStatus.Read).subscribe({
      next: () => {
        this.updateNotification(notificationId, { status: NotificationStatus.Read });
        
        // Update stats
        const currentStats = this.statsSubject.value;
        const updatedStats = {
          ...currentStats,
          unread: Math.max(0, currentStats.unread - 1),
          read: currentStats.read + 1
        };
        this.statsSubject.next(updatedStats);
      },
      error: (error) => {
        console.error('Error marking notification as read:', error);
      }
    });
  }

  /**
   * Mark notification as archived
   */
  markAsArchived(notificationId: number): void {
    this.updateNotificationStatus(notificationId, NotificationStatus.Archived).subscribe({
      next: () => {
        this.updateNotification(notificationId, { status: NotificationStatus.Archived });
        
        // Update stats
        const currentStats = this.statsSubject.value;
        const updatedStats = {
          ...currentStats,
          archived: currentStats.archived + 1
        };
        this.statsSubject.next(updatedStats);
      },
      error: (error) => {
        console.error('Error archiving notification:', error);
      }
    });
  }

  /**
   * Get current notifications
   */
  getCurrentNotifications(): NotificationDto[] {
    return this.notificationsSubject.value;
  }

  /**
   * Get current stats
   */
  getCurrentStats(): NotificationStatsDto {
    return this.statsSubject.value;
  }

  /**
   * Get unread count
   */
  getUnreadCount(): number {
    return this.statsSubject.value.unread;
  }

  /**
   * Check if there are unread notifications
   */
  hasUnreadNotifications(): boolean {
    return this.getUnreadCount() > 0;
  }

  /**
   * Get notifications by category
   */
  getNotificationsByCategory(category: NotificationCategory): NotificationDto[] {
    return this.notificationsSubject.value.filter(
      notification => notification.notificationType === category
    );
  }

  /**
   * Get notifications by priority
   */
  getNotificationsByPriority(priority: NotificationPriority): NotificationDto[] {
    return this.notificationsSubject.value.filter(
      notification => notification.priority === priority
    );
  }

  /**
   * Get high priority notifications
   */
  getHighPriorityNotifications(): NotificationDto[] {
    return this.notificationsSubject.value.filter(
      notification => notification.priority === NotificationPriority.High || 
                      notification.priority === NotificationPriority.Urgent
    );
  }
} 