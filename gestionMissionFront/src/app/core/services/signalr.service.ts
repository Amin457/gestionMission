import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NotificationDto, SignalRConnectionState, NotificationCategory, NotificationPriority, NotificationStatus } from '../models/notification.model';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection?: HubConnection;
  private connectionStateSubject = new BehaviorSubject<SignalRConnectionState>({
    isConnected: false,
    isConnecting: false,
    isReconnecting: false
  });

  private notificationSubject = new Subject<NotificationDto>();
  private connectionErrorSubject = new Subject<string>();

  public connectionState$ = this.connectionStateSubject.asObservable();
  public notifications$ = this.notificationSubject.asObservable();
  public connectionErrors$ = this.connectionErrorSubject.asObservable();

  constructor(private authService: AuthService) {}

  /**
   * Initialize SignalR connection
   */
  async initializeConnection(): Promise<void> {
    try {
      this.updateConnectionState({ isConnecting: true });

      // Get authentication token
      const token = this.authService.getToken();
      
      // Try different SignalR hub URLs
      const hubUrls = [
        `${environment.apiBaseUrl}/notificationHub`,
        `${environment.apiBaseUrl}/hubs/notificationHub`,
        `${environment.apiBaseUrl}/signalr/notificationHub`
      ];

      let connectionEstablished = false;
      let lastError: any = null;

      for (const hubUrl of hubUrls) {
        try {
          console.log(`Attempting to connect to SignalR hub: ${hubUrl}`);
          
          // Build connection with authentication
          const connectionBuilder = new HubConnectionBuilder()
            .withUrl(hubUrl, {
              accessTokenFactory: () => token || ''
            })
            .withAutomaticReconnect([0, 2000, 10000, 30000]) // Retry intervals
            .configureLogging(LogLevel.Information);

          this.hubConnection = connectionBuilder.build();

          // Set up event handlers
          this.setupEventHandlers();

          // Start connection
          await this.hubConnection.start();
          
          this.updateConnectionState({
            isConnected: true,
            isConnecting: false,
            connectionId: this.hubConnection.connectionId || undefined
          });

          console.log('SignalR connected successfully with connection ID:', this.hubConnection.connectionId);
          connectionEstablished = true;
          break;
        } catch (error) {
          console.warn(`Failed to connect to ${hubUrl}:`, error);
          lastError = error;
          continue;
        }
      }

      if (!connectionEstablished) {
        throw lastError || new Error('All SignalR hub URLs failed');
      }

    } catch (error) {
      console.error('SignalR connection failed:', error);
      this.updateConnectionState({
        isConnected: false,
        isConnecting: false,
        error: error instanceof Error ? error.message : 'Connection failed'
      });
      this.connectionErrorSubject.next('Failed to connect to notification service');
      
      // Log specific error details for debugging
      if (error instanceof Error) {
        if (error.message.includes('401')) {
          console.error('Authentication failed - check token validity');
        } else if (error.message.includes('404')) {
          console.error('Notification hub not found - check backend configuration');
          console.error('Backend needs to implement SignalR hub at: /notificationHub');
        } else if (error.message.includes('CORS')) {
          console.error('CORS error - check backend CORS configuration');
        }
      }
    }
  }

  /**
   * Set up SignalR event handlers
   */
  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    // Handle incoming notifications
    this.hubConnection.on('ReceiveNotification', (rawNotification: any) => {
      console.log('Received notification:', rawNotification);
      
      // Transform the notification to ensure it has all required properties
      const transformedNotification = this.transformNotificationData(rawNotification);
      
      this.notificationSubject.next(transformedNotification);
    });

    // Handle connection close
    this.hubConnection.onclose((error?: Error) => {
      console.log('SignalR connection closed:', error);
      this.updateConnectionState({
        isConnected: false,
        isConnecting: false,
        error: error?.message || 'Connection closed'
      });
    });

    // Handle reconnecting
    this.hubConnection.onreconnecting((error?: Error) => {
      console.log('SignalR reconnecting:', error);
      this.updateConnectionState({
        isConnected: false,
        isConnecting: false,
        isReconnecting: true,
        error: error?.message || 'Reconnecting'
      });
    });

    // Handle reconnected
    this.hubConnection.onreconnected((connectionId?: string) => {
      console.log('SignalR reconnected:', connectionId);
      this.updateConnectionState({
        isConnected: true,
        isConnecting: false,
        isReconnecting: false,
        connectionId
      });
    });
  }

  /**
   * Transform raw notification data to match NotificationDto interface
   */
  private transformNotificationData(rawData: any): NotificationDto {
    console.log('Raw notification data:', rawData);
    console.log('Priority value:', rawData.priority, 'Type:', typeof rawData.priority);
    console.log('Status value:', rawData.status, 'Type:', typeof rawData.status);
    console.log('Type value:', rawData.notificationType, 'Type:', typeof rawData.notificationType);
    
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
   * Join user-specific group for targeted notifications
   */
  async joinUserGroup(userId: number): Promise<void> {
    try {
      if (this.hubConnection && this.hubConnection.state === 'Connected') {
        await this.hubConnection.invoke('JoinUserGroup', userId);
        console.log(`Joined user group for user ${userId}`);
      }
    } catch (error) {
      console.warn('JoinUserGroup method not available on server, continuing without user groups:', error);
      // This is not a critical error - the connection will still work for general notifications
    }
  }

  /**
   * Leave user-specific group
   */
  async leaveUserGroup(userId: number): Promise<void> {
    try {
      if (this.hubConnection && this.hubConnection.state === 'Connected') {
        await this.hubConnection.invoke('LeaveUserGroup', userId);
        console.log(`Left user group for user ${userId}`);
      }
    } catch (error) {
      console.error('Failed to leave user group:', error);
    }
  }

  /**
   * Send a message to the hub (for testing purposes)
   */
  async sendMessage(message: string): Promise<void> {
    try {
      if (this.hubConnection && this.hubConnection.state === 'Connected') {
        await this.hubConnection.invoke('SendMessage', message);
      }
    } catch (error) {
      console.error('Failed to send message:', error);
    }
  }

  /**
   * Disconnect from SignalR
   */
  async disconnect(): Promise<void> {
    try {
      if (this.hubConnection) {
        await this.hubConnection.stop();
        this.updateConnectionState({
          isConnected: false,
          isConnecting: false,
          isReconnecting: false
        });
        console.log('SignalR disconnected');
      }
    } catch (error) {
      console.error('Error disconnecting from SignalR:', error);
    }
  }

  /**
   * Get current connection state
   */
  getConnectionState(): SignalRConnectionState {
    return this.connectionStateSubject.value;
  }

  /**
   * Check if connection is active
   */
  isConnected(): boolean {
    return this.connectionStateSubject.value.isConnected;
  }

  /**
   * Update connection state
   */
  private updateConnectionState(state: Partial<SignalRConnectionState>): void {
    const currentState = this.connectionStateSubject.value;
    this.connectionStateSubject.next({ ...currentState, ...state });
  }

  /**
   * Manually trigger reconnection
   */
  async reconnect(): Promise<void> {
    if (this.hubConnection) {
      await this.disconnect();
      await this.initializeConnection();
    }
  }
} 