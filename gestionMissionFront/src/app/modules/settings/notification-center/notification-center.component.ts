import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

// PrimeNG Components
import { DataViewModule } from 'primeng/dataview';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule } from 'primeng/paginator';
import { ChipModule } from 'primeng/chip';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { TooltipModule } from 'primeng/tooltip';

// Services
import { NotificationService } from '../../../core/services/notification.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { NotificationTestService } from '../../../core/services/notification-test.service';
 
// Models
import { 
  NotificationDto, 
  NotificationFilter, 
  NotificationStatus, 
  NotificationCategory, 
  NotificationPriority 
} from '../../../core/models/notification.model';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DataViewModule,
    DropdownModule,
    CalendarModule,
    ButtonModule,
    InputTextModule,
    PaginatorModule,
    ChipModule,
    ToastModule,
    ConfirmDialogModule,
    TooltipModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './notification-center.component.html',
  styleUrls: ['./notification-center.component.scss']
})
export class NotificationCenterComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  notifications: NotificationDto[] = [];
  filteredNotifications: NotificationDto[] = [];
  stats = {
    total: 0,
    unread: 0,
    read: 0,
    archived: 0
  };

  // Filter options
  statusOptions = [
    { label: 'All', value: null },
    { label: 'Unread', value: NotificationStatus.Unread },
    { label: 'Read', value: NotificationStatus.Read },
    { label: 'Archived', value: NotificationStatus.Archived }
  ];

  typeOptions = [
    { label: 'All Types', value: null },
    { label: 'Mission', value: NotificationCategory.Mission },
    { label: 'Task', value: NotificationCategory.Task },
    { label: 'Reservation', value: NotificationCategory.Reservation },
    { label: 'Incident', value: NotificationCategory.Incident },
    { label: 'System', value: NotificationCategory.System },
    { label: 'Alert', value: NotificationCategory.Alert }
  ];

  priorityOptions = [
    { label: 'All Priorities', value: null },
    { label: 'Urgent', value: NotificationPriority.Urgent },
    { label: 'High', value: NotificationPriority.High },
    { label: 'Normal', value: NotificationPriority.Normal },
    { label: 'Low', value: NotificationPriority.Low }
  ];

  // Filters
  currentFilter: NotificationFilter = {};
  searchText = '';

  // Pagination
  first = 0;
  rows = 20;
  totalRecords = 0;

  // Loading states
  isLoading = false;
  isFiltering = false;

  // View options
  layout: 'list' | 'grid' = 'list';

  constructor(
    private notificationService: NotificationService,
    private signalRService: SignalRService,
    private notificationTestService: NotificationTestService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadNotifications();
    this.loadStats();
    this.setupRealTimeUpdates();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Test notification system
   */
  async testNotificationSystem(): Promise<void> {
    this.messageService.add({
      severity: 'info',
      summary: 'Testing',
      detail: 'Testing notification system...'
    });

    const result = await this.notificationTestService.testNotificationSystem();
    
    this.messageService.add({
      severity: result.success ? 'success' : 'error',
      summary: result.success ? 'Success' : 'Error',
      detail: result.message
    });

    console.log('Test results:', result);
  }

  /**
   * Create test notification
   */
  async createTestNotification(): Promise<void> {
    const result = await this.notificationTestService.createTestNotification();
    
    this.messageService.add({
      severity: result.success ? 'success' : 'error',
      summary: result.success ? 'Success' : 'Error',
      detail: result.message
    });

    if (result.success) {
      this.loadNotifications();
      this.loadStats();
    }
  }

  /**
   * Send test real-time notification
   */
  async sendTestRealTimeNotification(): Promise<void> {
    const result = await this.notificationTestService.sendTestRealTimeNotification();
    
    this.messageService.add({
      severity: result.success ? 'success' : 'error',
      summary: result.success ? 'Success' : 'Error',
      detail: result.message
    });
  }

  /**
   * Load notifications
   */
  loadNotifications(): void {
    this.isLoading = true;
    
    this.notificationService.getNotifications(this.currentFilter).subscribe({
      next: (notifications) => {
        this.notifications = notifications;
        this.applySearchFilter();
        this.totalRecords = this.filteredNotifications.length;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading notifications:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load notifications'
        });
        this.isLoading = false;
      }
    });
  }

  /**
   * Load notification statistics
   */
  loadStats(): void {
    this.notificationService.getNotificationStats().subscribe({
      next: (stats) => {
        this.stats = stats;
      },
      error: (error) => {
        console.error('Error loading notification stats:', error);
      }
    });
  }

  /**
   * Setup real-time updates
   */
  setupRealTimeUpdates(): void {
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
    // Add to the beginning of the list
    this.notifications.unshift(notification);
    this.applySearchFilter();
    this.totalRecords = this.filteredNotifications.length;
    
    // Update stats
    this.stats.total++;
    this.stats.unread++;
  }

  /**
   * Apply search filter
   */
  applySearchFilter(): void {
    if (!this.searchText.trim()) {
      this.filteredNotifications = [...this.notifications];
    } else {
      const searchLower = this.searchText.toLowerCase();
      this.filteredNotifications = this.notifications.filter(notification =>
        notification.title.toLowerCase().includes(searchLower) ||
        notification.message.toLowerCase().includes(searchLower) ||
        notification.notificationType.toLowerCase().includes(searchLower)
      );
    }
  }

  /**
   * Apply filters
   */
  applyFilters(): void {
    this.isFiltering = true;
    this.currentFilter = {
      status: this.currentFilter.status,
      type: this.currentFilter.type,
      priority: this.currentFilter.priority,
      dateFrom: this.currentFilter.dateFrom,
      dateTo: this.currentFilter.dateTo
    };

    this.loadNotifications();
  }

  /**
   * Clear all filters
   */
  clearFilters(): void {
    this.currentFilter = {};
    this.searchText = '';
    this.first = 0;
    this.loadNotifications();
  }

  /**
   * Mark notification as read
   */
  markAsRead(notification: NotificationDto): void {
    this.notificationService.markAsRead(notification.notificationId);
    
    // Update local state
    const index = this.notifications.findIndex(n => n.notificationId === notification.notificationId);
    if (index !== -1) {
      this.notifications[index].status = NotificationStatus.Read;
      this.applySearchFilter();
    }
    
    // Update stats
    this.stats.unread = Math.max(0, this.stats.unread - 1);
    this.stats.read++;
  }

  /**
   * Mark notification as archived
   */
  markAsArchived(notification: NotificationDto): void {
    this.notificationService.markAsArchived(notification.notificationId);
    
    // Update local state
    const index = this.notifications.findIndex(n => n.notificationId === notification.notificationId);
    if (index !== -1) {
      this.notifications[index].status = NotificationStatus.Archived;
      this.applySearchFilter();
    }
    
    // Update stats
    this.stats.archived++;
  }

  /**
   * Delete notification
   */
  deleteNotification(notification: NotificationDto): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete this notification?',
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.notificationService.deleteNotification(notification.notificationId).subscribe({
          next: () => {
            // Remove from local state
            const index = this.notifications.findIndex(n => n.notificationId === notification.notificationId);
            if (index !== -1) {
              this.notifications.splice(index, 1);
              this.applySearchFilter();
              this.totalRecords = this.filteredNotifications.length;
            }
            
            // Update stats
            this.stats.total--;
            if (notification.status === NotificationStatus.Unread) {
              this.stats.unread = Math.max(0, this.stats.unread - 1);
            }
            
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Notification deleted successfully'
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
    });
  }

  /**
   * Mark all notifications as read
   */
  markAllAsRead(): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to mark all notifications as read?',
      header: 'Mark All as Read',
      icon: 'pi pi-check-circle',
      accept: () => {
        this.notificationService.markAllAsRead().subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'All notifications marked as read'
            });
            // The notification service will update the local state automatically
            // and the subscription will handle the UI updates
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
    });
  }

  /**
   * Navigate to notification details
   */
  navigateToNotification(notification: NotificationDto): void {
    // Mark as read if unread
    if (notification.status === NotificationStatus.Unread) {
      this.markAsRead(notification);
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
          // For system notifications and alerts, stay on notification center
          this.messageService.add({
            severity: 'info',
            summary: 'Info',
            detail: 'This notification does not have a specific action page'
          });
          break;
      }
    } else {
      this.messageService.add({
        severity: 'info',
        summary: 'Info',
        detail: 'This notification does not have a specific action page'
      });
    }
  }

  /**
   * Get notification icon
   */
  getNotificationIcon(notification: NotificationDto): string {
    switch (notification.notificationType) {
      case NotificationCategory.Mission:
        return 'pi pi-send';
      case NotificationCategory.Task:
        return 'pi pi-list';
      case NotificationCategory.Reservation:
        return 'pi pi-calendar';
      case NotificationCategory.Incident:
        return 'pi pi-exclamation-triangle';
      case NotificationCategory.System:
        return 'pi pi-cog';
      case NotificationCategory.Alert:
        return 'pi pi-bell';
      default:
        return 'pi pi-info-circle';
    }
  }

  /**
   * Get priority severity
   */
  getPrioritySeverity(priority: NotificationPriority): 'danger' | 'warning' | 'info' | 'success' {
    switch (priority) {
      case NotificationPriority.Urgent:
        return 'danger';
      case NotificationPriority.High:
        return 'warning';
      case NotificationPriority.Normal:
        return 'info';
      case NotificationPriority.Low:
        return 'success';
      default:
        return 'info';
    }
  }

  /**
   * Get status severity
   */
  getStatusSeverity(status: NotificationStatus): 'danger' | 'warning' | 'info' | 'success' {
    switch (status) {
      case NotificationStatus.Unread:
        return 'danger';
      case NotificationStatus.Read:
        return 'success';
      case NotificationStatus.Archived:
        return 'info';
      default:
        return 'info';
    }
  }

  /**
   * Format notification date
   */
  formatNotificationDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  /**
   * Get time ago
   */
  getTimeAgo(dateString: string): string {
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
   * Handle pagination change
   */
  onPageChange(event: any): void {
    this.first = event.first;
    this.rows = event.rows;
  }

  /**
   * Toggle layout
   */
  toggleLayout(): void {
    this.layout = this.layout === 'list' ? 'grid' : 'list';
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
} 