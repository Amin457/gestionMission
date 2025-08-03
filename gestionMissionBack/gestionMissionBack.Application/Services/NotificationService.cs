using gestionMissionBack.Application.DTOs.Notification;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserConnectionRepository _userConnectionRepository;
        private readonly ISignalRService _signalRService;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUserConnectionRepository userConnectionRepository,
            ISignalRService signalRService)
        {
            _notificationRepository = notificationRepository;
            _userConnectionRepository = userConnectionRepository;
            _signalRService = signalRService;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createDto)
        {
            var notification = new Notification
            {
                UserId = createDto.UserId,
                Title = createDto.Title,
                Message = createDto.Message,
                SentDate = DateTime.UtcNow,
                NotificationType = createDto.NotificationType,
                Priority = createDto.Priority,
                Status = NotificationStatus.Unread,
                RelatedEntityType = createDto.RelatedEntityType,
                RelatedEntityId = createDto.RelatedEntityId,
                ExpiryDate = createDto.ExpiryDate
            };

            await _notificationRepository.AddAsync(notification);

            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                SentDate = notification.SentDate,
                NotificationType = notification.NotificationType,
                Priority = notification.Priority,
                Status = notification.Status,
                RelatedEntityType = notification.RelatedEntityType,
                RelatedEntityId = notification.RelatedEntityId,
                ExpiryDate = notification.ExpiryDate
            };
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, NotificationFilter filter)
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, filter);
            
            return notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                SentDate = n.SentDate,
                NotificationType = n.NotificationType,
                Priority = n.Priority,
                Status = n.Status,
                RelatedEntityType = n.RelatedEntityType,
                RelatedEntityId = n.RelatedEntityId,
                ExpiryDate = n.ExpiryDate
            });
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            
            if (notification == null || notification.UserId != userId)
                return null;

            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                SentDate = notification.SentDate,
                NotificationType = notification.NotificationType,
                Priority = notification.Priority,
                Status = notification.Status,
                RelatedEntityType = notification.RelatedEntityType,
                RelatedEntityId = notification.RelatedEntityId,
                ExpiryDate = notification.ExpiryDate
            };
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            return await _notificationRepository.MarkAsReadAsync(notificationId, userId);
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            return await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task<bool> UpdateNotificationStatusAsync(int notificationId, int userId, NotificationStatus status)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            
            if (notification == null || notification.UserId != userId)
                return false;

            notification.Status = status;
            await _notificationRepository.UpdateAsync(notification);
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            
            if (notification == null || notification.UserId != userId)
                return false;

            await _notificationRepository.DeleteAsync(notificationId);
            return true;
        }

        public async Task<NotificationStatsDto> GetNotificationStatsAsync(int userId)
        {
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);
            var allNotifications = await _notificationRepository.GetUserNotificationsAsync(userId, new NotificationFilter());
            
            return new NotificationStatsDto
            {
                Total = allNotifications.Count(),
                Unread = unreadCount,
                Read = allNotifications.Count(n => n.Status == NotificationStatus.Read),
                Archived = allNotifications.Count(n => n.Status == NotificationStatus.Archived)
            };
        }

        public async Task SendRealTimeNotificationAsync(int userId, CreateNotificationDto notification)
        {
            // Create and save notification
            var createdNotification = await CreateNotificationAsync(notification);

            // Get user connections
            var userConnections = await _userConnectionRepository.GetUserConnectionsAsync(userId);

            // Send to all user connections
            foreach (var connection in userConnections)
            {
                await _signalRService.SendToUserAsync(connection.ConnectionId, createdNotification);
            }
        }

        public async Task SendRealTimeNotificationToAllAsync(CreateNotificationDto notification)
        {
            // Create and save notification
            var createdNotification = await CreateNotificationAsync(notification);

            // Send to all connections
            await _signalRService.SendToAllAsync(createdNotification);
        }

        public async Task SendRealTimeNotificationToRoleAsync(string roleCode, CreateNotificationDto notification)
        {
            // Create and save notification first
            var createdNotification = await CreateNotificationAsync(notification);
            
            // This would require additional logic to get users by role
            // For now, we'll implement a simplified version
            // In a real implementation, you'd query users by role and send to their connections
            
            var allConnections = await _userConnectionRepository.GetAllAsync();
            
            // Send to all connections (in a real implementation, filter by role)
            foreach (var connection in allConnections)
            {
                await _signalRService.SendToUserAsync(connection.ConnectionId, createdNotification);
            }
        }
    }
} 