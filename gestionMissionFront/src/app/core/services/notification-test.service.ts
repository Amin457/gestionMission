import { Injectable } from '@angular/core';
import { NotificationService } from './notification.service';
import { SignalRService } from './signalr.service';
import { AuthService } from './auth.service';
import { CreateNotificationDto, NotificationCategory, NotificationPriority } from '../models/notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationTestService {

  constructor(
    private notificationService: NotificationService,
    private signalRService: SignalRService,
    private authService: AuthService
  ) {}

  /**
   * Test notification system connectivity
   */
  async testNotificationSystem(): Promise<{ success: boolean; message: string; details?: any }> {
    const results: {
      auth: boolean;
      api: boolean;
      signalR: boolean;
      details: {
        auth?: any;
        api?: any;
        signalR?: any;
      };
    } = {
      auth: false,
      api: false,
      signalR: false,
      details: {}
    };

    try {
      // Test 1: Authentication
      const token = this.authService.getToken();
      const currentUser = this.authService.getCurrentUser();
      
      if (token && currentUser) {
        results.auth = true;
        results.details.auth = { token: token.substring(0, 20) + '...', userId: currentUser.userId };
      } else {
        results.details.auth = { error: 'No token or user found' };
      }

      // Test 2: API Connectivity
      try {
        await this.notificationService.getNotificationStats().toPromise();
        results.api = true;
        results.details.api = { status: 'API endpoint accessible' };
      } catch (error: any) {
        results.details.api = { 
          error: error.message, 
          status: error.status,
          statusText: error.statusText 
        };
      }

      // Test 3: SignalR Connectivity
      try {
        await this.signalRService.initializeConnection();
        const connectionState = this.signalRService.getConnectionState();
        results.signalR = connectionState.isConnected;
        results.details.signalR = { 
          connected: connectionState.isConnected,
          connectionId: connectionState.connectionId,
          error: connectionState.error
        };
      } catch (error: any) {
        results.details.signalR = { error: error.message };
      }

      // Determine overall success
      const success = results.auth && results.api && results.signalR;
      const message = success 
        ? 'All notification system components are working correctly!'
        : 'Some notification system components have issues. Check details below.';

      return { success, message, details: results };

    } catch (error: any) {
      return { 
        success: false, 
        message: 'Failed to test notification system', 
        details: { error: error.message } 
      };
    }
  }

  /**
   * Create a test notification
   */
  createTestNotification(): Promise<{ success: boolean; message: string }> {
    const currentUser = this.authService.getCurrentUser();
    
    if (!currentUser) {
      return Promise.resolve({ 
        success: false, 
        message: 'No authenticated user found' 
      });
    }

    const testNotification: CreateNotificationDto = {
      userId: currentUser.userId,
      title: 'Test Notification',
      message: 'This is a test notification created at ' + new Date().toLocaleString(),
      notificationType: NotificationCategory.System,
      priority: NotificationPriority.Normal,
      relatedEntityType: 'Test',
      relatedEntityId: 1
    };

    return new Promise((resolve) => {
      this.notificationService.createNotification(testNotification).subscribe({
        next: (notification) => {
          console.log('Test notification created:', notification);
          resolve({ 
            success: true, 
            message: 'Test notification created successfully' 
          });
        },
        error: (error) => {
          console.error('Failed to create test notification:', error);
          resolve({ 
            success: false, 
            message: 'Failed to create test notification: ' + error.message 
          });
        }
      });
    });
  }

  /**
   * Send a test real-time notification
   */
  sendTestRealTimeNotification(): Promise<{ success: boolean; message: string }> {
    const currentUser = this.authService.getCurrentUser();
    
    if (!currentUser) {
      return Promise.resolve({ 
        success: false, 
        message: 'No authenticated user found' 
      });
    }

    const testNotification = {
      userId: currentUser.userId,
      title: 'Real-time Test',
      message: 'This is a real-time test notification sent at ' + new Date().toLocaleString(),
      notificationType: NotificationCategory.System,
      priority: NotificationPriority.High
    };

    return new Promise((resolve) => {
      this.notificationService.sendRealTimeNotification(testNotification).subscribe({
        next: () => {
          console.log('Test real-time notification sent');
          resolve({ 
            success: true, 
            message: 'Test real-time notification sent successfully' 
          });
        },
        error: (error) => {
          console.error('Failed to send test real-time notification:', error);
          resolve({ 
            success: false, 
            message: 'Failed to send test real-time notification: ' + error.message 
          });
        }
      });
    });
  }

  /**
   * Get system status
   */
  getSystemStatus(): { auth: boolean; api: boolean; signalR: boolean } {
    const token = this.authService.getToken();
    const currentUser = this.authService.getCurrentUser();
    const signalRState = this.signalRService.getConnectionState();

    return {
      auth: !!(token && currentUser),
      api: true, // We'll test this when needed
      signalR: signalRState.isConnected
    };
  }
} 