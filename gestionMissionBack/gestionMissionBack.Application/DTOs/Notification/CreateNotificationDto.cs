using System;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.DTOs.Notification
{
    public class CreateNotificationDto
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationCategory NotificationType { get; set; }
        public NotificationPriority Priority { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
} 