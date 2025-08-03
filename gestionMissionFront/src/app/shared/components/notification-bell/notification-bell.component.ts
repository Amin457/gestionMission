import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

// PrimeNG Components
import { BadgeModule } from 'primeng/badge';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

// Services
import { NotificationService } from '../../../core/services/notification.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { AuthService } from '../../../core/services/auth.service';

// Models
import { NotificationDto, NotificationStatus, NotificationPriority } from '../../../core/models/notification.model';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [
    CommonModule,
    BadgeModule,
    OverlayPanelModule,
    ButtonModule,
    TooltipModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './notification-bell.component.html',
  styleUrls: ['./notification-bell.component.scss']
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Notification data
  notifications: NotificationDto[] = [];
  unreadCount = 0;
  isLoading = false;

  // UI state
  isOverlayVisible = false;
  connectionStatus = 'disconnected';

  constructor(
    private notificationService: NotificationService,
    private signalRService: SignalRService,
    private authService: AuthService,
    private messageService: MessageService,
    private router: Router
  ) {}

  ngOnInit() {
    this.initializeNotifications();
    this.setupSignalR();
    this.subscribeToNotifications();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Initialize notifications
   */
  private initializeNotifications(): void {
    this.isLoading = true;
    
    // Load notifications
    this.notificationService.loadNotifications();
    
    // Load stats
    this.notificationService.loadNotificationStats();
  }

  /**
   * Setup SignalR connection
   */
  private setupSignalR(): void {
    // Initialize SignalR connection
    this.signalRService.initializeConnection().then(() => {
      console.log('SignalR connection established successfully');
    }).catch((error) => {
      console.warn('SignalR connection failed, notifications will work without real-time updates:', error);
      // Set connection status to show it's not available
      this.connectionStatus = 'disconnected';
    });

    // Subscribe to connection state
    this.signalRService.connectionState$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(state => {
      this.connectionStatus = state.isConnected ? 'connected' : 
                             state.isConnecting ? 'connecting' : 
                             state.isReconnecting ? 'reconnecting' : 'disconnected';
    });

    // Subscribe to connection errors
    this.signalRService.connectionErrors$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(error => {
      console.warn('SignalR connection error:', error);
      this.connectionStatus = 'disconnected';
    });
  }

  /**
   * Subscribe to notification updates
   */
  private subscribeToNotifications(): void {
    // Subscribe to notifications
    this.notificationService.notifications$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(notifications => {
      this.notifications = notifications;
      this.isLoading = false;
    });

    // Subscribe to stats
    this.notificationService.stats$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(stats => {
      this.unreadCount = stats.unread;
    });

    // Subscribe to real-time notifications
    this.signalRService.notifications$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(notification => {
      this.handleNewNotification(notification);
    });
  }

  /**
   * Handle new real-time notification
   */
  private handleNewNotification(notification: NotificationDto): void {
    // Add to local state
    this.notificationService.addNotification(notification);
    
    // Show toast notification
    this.showNotificationToast(notification);
    
    // Play sound for high priority notifications
    if (notification.priority && 
        (notification.priority === NotificationPriority.High || 
         notification.priority === NotificationPriority.Urgent)) {
      this.playNotificationSound();
    }
  }

  /**
   * Show notification toast
   */
  private showNotificationToast(notification: NotificationDto): void {
    const severity = notification.priority ? this.getPrioritySeverity(notification.priority) : 'info';
    
    this.messageService.add({
      key: 'notification-toast',
      severity,
      summary: notification.title,
      detail: notification.message,
      life: 5000,
      data: notification
    });
  }

  /**
   * Play notification sound
   */
  private playNotificationSound(): void {
    // Create audio element and play notification sound
    const audio = new Audio('assets/sounds/notification.mp3');
    audio.volume = 0.5;
    audio.play().catch(error => {
      console.log('Could not play notification sound:', error);
    });
  }

  /**
   * Get priority severity for PrimeNG
   */
  private getPrioritySeverity(priority: NotificationPriority): 'info' | 'success' | 'warn' | 'error' {
    switch (priority) {
      case NotificationPriority.Urgent:
        return 'error';
      case NotificationPriority.High:
        return 'warn';
      case NotificationPriority.Normal:
        return 'info';
      case NotificationPriority.Low:
        return 'success';
      default:
        return 'info';
    }
  }

  /**
   * Toggle notification overlay
   */
  toggleOverlay(event: Event, overlayPanel: any): void {
    if (this.isOverlayVisible) {
      overlayPanel.hide();
    } else {
      overlayPanel.show(event);
    }
    this.isOverlayVisible = !this.isOverlayVisible;
  }

  /**
   * Mark notification as read
   */
  markAsRead(notification: NotificationDto, event: Event): void {
    event.stopPropagation();
    
    if ((notification.status || NotificationStatus.Unread) === NotificationStatus.Unread) {
      this.notificationService.markAsRead(notification.notificationId);
    }
  }

  /**
   * Mark all notifications as read
   */
  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notificationService.loadNotificationStats();
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'All notifications marked as read'
        });
      },
      error: (error) => {
        console.error('Error marking all as read:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to mark notifications as read'
        });
      }
    });
  }

  /**
   * Delete notification
   */
  deleteNotification(notification: NotificationDto, event: Event): void {
    event.stopPropagation();
    
    this.notificationService.deleteNotification(notification.notificationId).subscribe({
      next: () => {
        this.notificationService.removeNotification(notification.notificationId);
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Notification deleted'
        });
      },
      error: (error) => {
        console.error('Error deleting notification:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to delete notification'
        });
      }
    });
  }

  /**
   * Navigate to notification details
   */
  navigateToNotification(notification: NotificationDto): void {
    // Mark as read if unread
    if ((notification.status || NotificationStatus.Unread) === NotificationStatus.Unread) {
      this.notificationService.markAsRead(notification.notificationId);
    }

    // Navigate based on notification type
    if (notification.relatedEntityType && notification.relatedEntityId) {
      switch (notification.relatedEntityType) {
        case 'Mission':
          this.router.navigate(['/missions/sheet', notification.relatedEntityId]);
          break;
        case 'Task':
          // For tasks, we need to navigate to the mission tasks page
          // The relatedEntityId should be the missionId for tasks
          this.router.navigate(['/missions/tasks', notification.relatedEntityId]);
          break;
        case 'Reservation':
          this.router.navigate(['/vehicles/reservation']);
          break;
        case 'Incident':
          this.router.navigate(['/missions/incidents']);
          break;
        case 'Vehicle':
          this.router.navigate(['/data/vehicules']);
          break;
        case 'System':
        case 'Alert':
        default:
          // For system notifications and alerts, navigate to notification history
          this.router.navigate(['/settings/notifications/history']);
          break;
      }
    } else {
      // Navigate to notification history
      this.router.navigate(['/settings/notifications/history']);
    }
  }

  /**
   * Get notification icon based on type
   */
  getNotificationIcon(notification: NotificationDto): string {
    if (!notification.notificationType) {
      return 'pi pi-info-circle';
    }
    
    switch (notification.notificationType) {
      case 'Mission':
        return 'pi pi-send';
      case 'Task':
        return 'pi pi-list';
      case 'Reservation':
        return 'pi pi-calendar';
      case 'Incident':
        return 'pi pi-exclamation-triangle';
      case 'System':
        return 'pi pi-cog';
      case 'Alert':
        return 'pi pi-bell';
      default:
        return 'pi pi-info-circle';
    }
  }

  /**
   * Get notification priority class
   */
  getNotificationPriorityClass(notification: NotificationDto): string {
    if (!notification.priority) {
      return 'priority-normal';
    }
    
    switch (notification.priority) {
      case NotificationPriority.Urgent:
        return 'priority-urgent';
      case NotificationPriority.High:
        return 'priority-high';
      case NotificationPriority.Normal:
        return 'priority-normal';
      case NotificationPriority.Low:
        return 'priority-low';
      default:
        return 'priority-normal';
    }
  }

  /**
   * Get priority string for CSS class (safe conversion)
   */
  getPriorityString(priority: any): string {
    console.log('getPriorityString called with:', priority, 'Type:', typeof priority);
    
    if (!priority) return 'normal';
    
    // If it's already a string, use it directly
    if (typeof priority === 'string') {
      return priority.toLowerCase();
    }
    
    // If it's a number, map it to the correct priority
    if (typeof priority === 'number') {
      switch (priority) {
        case 0: return 'low';
        case 1: return 'normal';
        case 2: return 'high';
        case 3: return 'urgent';
        default: return 'normal';
      }
    }
    
    // Convert to string and handle various input types
    const priorityStr = String(priority).toLowerCase();
    console.log('Converted priority string:', priorityStr);
    
    // Map common priority values
    switch (priorityStr) {
      case 'urgent':
      case 'high':
      case 'normal':
      case 'low':
        return priorityStr;
      default:
        return 'normal';
    }
  }

  /**
   * Format notification date
   */
  formatNotificationDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));
    
    if (diffInMinutes < 1) {
      return 'Just now';
    } else if (diffInMinutes < 60) {
      return `${diffInMinutes}m ago`;
    } else if (diffInMinutes < 1440) {
      const hours = Math.floor(diffInMinutes / 60);
      return `${hours}h ago`;
    } else {
      const days = Math.floor(diffInMinutes / 1440);
      return `${days}d ago`;
    }
  }

  /**
   * Check if notification is recent (within last 5 minutes)
   */
  isRecentNotification(notification: NotificationDto): boolean {
    const date = new Date(notification.sentDate);
    const now = new Date();
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));
    return diffInMinutes < 5;
  }

  /**
   * Navigate to notifications center
   */
  navigateToNotifications(): void {
    this.router.navigate(['/settings/notifications']);
  }
} 