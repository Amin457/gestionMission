// Notification Enums
export enum NotificationStatus {
  Unread = 'Unread',
  Read = 'Read',
  Archived = 'Archived'
}

export enum NotificationPriority {
  Low = 'Low',
  Normal = 'Normal',
  High = 'High',
  Urgent = 'Urgent'
}

export enum NotificationCategory {
  Mission = 'Mission',
  Task = 'Task',
  Reservation = 'Reservation',
  Incident = 'Incident',
  System = 'System',
  Alert = 'Alert'
}

export enum DeliveryMethod {
  Email = 'Email',
  Push = 'Push'
}

export enum DeliveryStatus {
  Sent = 'Sent',
  Delivered = 'Delivered',
  Failed = 'Failed'
}

// Notification Interfaces
export interface NotificationDto {
  notificationId: number;
  userId: number;
  title: string;
  message: string;
  sentDate: string;
  notificationType: NotificationCategory;
  priority: NotificationPriority;
  status: NotificationStatus;
  relatedEntityType?: string;
  relatedEntityId?: number;
  expiryDate?: string;
}

export interface CreateNotificationDto {
  userId: number;
  title: string;
  message: string;
  notificationType: NotificationCategory;
  priority: NotificationPriority;
  relatedEntityType?: string;
  relatedEntityId?: number;
  expiryDate?: string;
}

export interface NotificationFilter {
  status?: NotificationStatus;
  type?: NotificationCategory;
  priority?: NotificationPriority;
  dateFrom?: string;
  dateTo?: string;
  relatedEntityType?: string;
  relatedEntityId?: number;
}

export interface NotificationStatsDto {
  total: number;
  unread: number;
  read: number;
  archived: number;
}

export interface UpdateNotificationStatusDto {
  status: NotificationStatus;
}

export interface RealTimeNotificationDto {
  userId: number;
  title: string;
  message: string;
  notificationType: NotificationCategory;
  priority: NotificationPriority;
  relatedEntityType?: string;
  relatedEntityId?: number;
}

// SignalR Hub Interfaces
export interface SignalRConnectionState {
  isConnected: boolean;
  isConnecting: boolean;
  isReconnecting: boolean;
  connectionId?: string;
  error?: string;
}

export interface NotificationPreferences {
  emailNotifications: boolean;
  pushNotifications: boolean;
  missionNotifications: boolean;
  taskNotifications: boolean;
  reservationNotifications: boolean;
  incidentNotifications: boolean;
  systemNotifications: boolean;
  alertNotifications: boolean;
  quietHours: {
    enabled: boolean;
    startTime: string;
    endTime: string;
  };
} 