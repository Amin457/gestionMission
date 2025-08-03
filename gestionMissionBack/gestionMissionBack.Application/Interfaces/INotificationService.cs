using gestionMissionBack.Application.DTOs.Notification;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createDto);
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, NotificationFilter filter);
        Task<NotificationDto?> GetNotificationByIdAsync(int notificationId, int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> UpdateNotificationStatusAsync(int notificationId, int userId, NotificationStatus status);
        Task<bool> DeleteNotificationAsync(int notificationId, int userId);
        Task<NotificationStatsDto> GetNotificationStatsAsync(int userId);
        
        // Real-time notification methods
        Task SendRealTimeNotificationAsync(int userId, CreateNotificationDto notification);
        Task SendRealTimeNotificationToAllAsync(CreateNotificationDto notification);
        Task SendRealTimeNotificationToRoleAsync(string roleCode, CreateNotificationDto notification);
    }
} 