import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { NotificationService } from '../../../core/services/notification.service';
import { NotificationDto as Notification } from '../../../core/models/notification.model';
import { SharedModule } from '../../../shared/shared.module';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-notification-history',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SharedModule,
    ButtonModule,
    TableModule,
    TagModule,
    DropdownModule,
    InputTextModule,
    CardModule,
    TooltipModule,
    ConfirmDialogModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './notification-history.component.html',
  styleUrls: ['./notification-history.component.scss']
})
export class NotificationHistoryComponent implements OnInit, OnDestroy {
  notifications: Notification[] = [];
  filteredNotifications: Notification[] = [];
  loading: boolean = false;
  
  // Filter options
  filterText: string = '';
  filterType: string = '';
  filterStatus: string = '';
  
  typeOptions = [
    { label: 'All Types', value: '' },
    { label: 'Mission', value: 'Mission' },
    { label: 'Task', value: 'Task' },
    { label: 'Reservation', value: 'Reservation' },
    { label: 'Incident', value: 'Incident' },
    { label: 'System', value: 'System' },
    { label: 'Alert', value: 'Alert' }
  ];
  
  statusOptions = [
    { label: 'All Status', value: '' },
    { label: 'Unread', value: 'unread' },
    { label: 'Read', value: 'read' },
    { label: 'Archived', value: 'archived' }
  ];

  private subscription: Subscription = new Subscription();

  constructor(
    private notificationService: NotificationService,
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    // Load notifications and subscribe to real-time updates
    this.notificationService.loadNotifications();
    
    this.subscription.add(
      this.notificationService.notifications$.subscribe(notifications => {
        this.notifications = notifications;
        this.applyFilters();
        this.loading = false;
      })
    );
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }



  applyFilters() {
    this.filteredNotifications = this.notifications.filter(notification => {
      const matchesText = !this.filterText || 
        notification.title.toLowerCase().includes(this.filterText.toLowerCase()) ||
        notification.message.toLowerCase().includes(this.filterText.toLowerCase());
      
      const mappedType = this.mapNotificationType(notification.notificationType);
      const matchesType = !this.filterType || mappedType === this.filterType;
      
      const mappedStatus = this.mapNotificationStatus(notification.status);
      const matchesStatus = !this.filterStatus || 
        (this.filterStatus === 'read' && mappedStatus === 'Read') ||
        (this.filterStatus === 'unread' && mappedStatus === 'Unread') ||
        (this.filterStatus === 'archived' && mappedStatus === 'Archived');
      
      return matchesText && matchesType && matchesStatus;
    });
  }

  onFilterChange() {
    this.applyFilters();
  }

  clearFilters() {
    this.filterText = '';
    this.filterType = '';
    this.filterStatus = '';
    this.applyFilters();
  }

  /**
   * Map numeric notification type to string
   */
  mapNotificationType(type: any): string {
    if (typeof type === 'string') {
      return type;
    }
    
    switch (Number(type)) {
      case 0: return 'Mission';
      case 1: return 'Task';
      case 2: return 'Reservation';
      case 3: return 'Incident';
      case 4: return 'System';
      case 5: return 'Alert';
      default: return 'System';
    }
  }

  /**
   * Map numeric priority to string
   */
  mapNotificationPriority(priority: any): string {
    if (typeof priority === 'string') {
      return priority;
    }
    
    switch (Number(priority)) {
      case 0: return 'Low';
      case 1: return 'Normal';
      case 2: return 'High';
      case 3: return 'Urgent';
      default: return 'Normal';
    }
  }

  /**
   * Map numeric status to string
   */
  mapNotificationStatus(status: any): string {
    if (typeof status === 'string') {
      return status;
    }
    
    switch (Number(status)) {
      case 0: return 'Unread';
      case 1: return 'Read';
      case 2: return 'Archived';
      default: return 'Unread';
    }
  }

  /**
   * Check if notification is unread
   */
  isUnread(notification: Notification): boolean {
    const mappedStatus = this.mapNotificationStatus(notification.status);
    return mappedStatus === 'Unread';
  }

  /**
   * Check if notification is read
   */
  isRead(notification: Notification): boolean {
    const mappedStatus = this.mapNotificationStatus(notification.status);
    return mappedStatus === 'Read';
  }

  /**
   * Check if notification is archived
   */
  isArchived(notification: Notification): boolean {
    const mappedStatus = this.mapNotificationStatus(notification.status);
    return mappedStatus === 'Archived';
  }

  getTypeClass(type: any): "info" | "success" | "warning" | "secondary" | "danger" | "contrast" | undefined {
    const mappedType = this.mapNotificationType(type);
    
    switch (mappedType) {
      case 'Mission': return 'info';
      case 'Task': return 'success';
      case 'Reservation': return 'warning';
      case 'Incident': return 'danger';
      case 'System': return 'secondary';
      case 'Alert': return 'danger';
      default: return 'info';
    }
  }

  getTypeIcon(type: any): string {
    const mappedType = this.mapNotificationType(type);
    
    switch (mappedType) {
      case 'Mission': return 'pi pi-send';
      case 'Task': return 'pi pi-list';
      case 'Reservation': return 'pi pi-calendar';
      case 'Incident': return 'pi pi-exclamation-triangle';
      case 'System': return 'pi pi-cog';
      case 'Alert': return 'pi pi-bell';
      default: return 'pi pi-info-circle';
    }
  }

  formatDate(date: Date | string): string {
    try {
      const dateObj = new Date(date);
      
      // Check if the date is valid
      if (isNaN(dateObj.getTime())) {
        return 'Invalid Date';
      }
      
      return new Intl.DateTimeFormat('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      }).format(dateObj);
    } catch (error) {
      console.warn('Error formatting date:', error, 'Date value:', date);
      return 'Invalid Date';
    }
  }

  markAsRead(notification: Notification) {
    const mappedStatus = this.mapNotificationStatus(notification.status);
    if (mappedStatus === 'Unread') {
      this.notificationService.markAsRead(notification.notificationId);
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Notification marked as read'
      });
      // The notification service will update the local state automatically
      // and the subscription will handle the UI updates
    }
  }

  markAllAsRead() {
    this.confirmationService.confirm({
      message: 'Are you sure you want to mark all notifications as read?',
      header: 'Confirm Action',
      icon: 'pi pi-question-circle',
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
          error: (error: any) => {
            console.error('Error marking all notifications as read:', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to mark all notifications as read'
            });
          }
        });
      }
    });
  }

  archiveNotification(notification: Notification) {
    const mappedStatus = this.mapNotificationStatus(notification.status);
    if (mappedStatus !== 'Archived') {
      this.confirmationService.confirm({
        message: `Are you sure you want to archive this notification?`,
        header: 'Confirm Archive',
        icon: 'pi pi-inbox',
        acceptButtonStyleClass: 'p-button-secondary',
        accept: () => {
          this.notificationService.markAsArchived(notification.notificationId);
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Notification archived successfully'
          });
          // The notification service will update the local state automatically
          // and the subscription will handle the UI updates
        }
      });
    }
  }

  navigateToAction(notification: Notification) {
    if (notification.relatedEntityType && notification.relatedEntityId) {
      this.markAsRead(notification);
      // Navigate based on entity type
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
          // For system notifications and alerts, stay on notification history
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

  getUnreadCount(): number {
    return this.notifications.filter(n => this.mapNotificationStatus(n.status) === 'Unread').length;
  }

  getReadCount(): number {
    return this.notifications.filter(n => this.mapNotificationStatus(n.status) === 'Read').length;
  }

  getArchivedCount(): number {
    return this.notifications.filter(n => this.mapNotificationStatus(n.status) === 'Archived').length;
  }
}