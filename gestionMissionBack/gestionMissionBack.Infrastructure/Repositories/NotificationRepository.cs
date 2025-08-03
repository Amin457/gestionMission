using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Persistence;
using gestionMissionBack.Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestionMissionBack.Infrastructure.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(MissionFleetContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, NotificationFilter filter)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (filter.Status.HasValue)
                query = query.Where(n => n.Status == filter.Status.Value);

            if (filter.Type.HasValue)
                query = query.Where(n => n.NotificationType == filter.Type.Value);

            if (filter.Priority.HasValue)
                query = query.Where(n => n.Priority == filter.Priority.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(n => n.SentDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(n => n.SentDate <= filter.DateTo.Value);

            if (!string.IsNullOrEmpty(filter.RelatedEntityType))
                query = query.Where(n => n.RelatedEntityType == filter.RelatedEntityType);

            if (filter.RelatedEntityId.HasValue)
                query = query.Where(n => n.RelatedEntityId == filter.RelatedEntityId.Value);

            return await query
                .OrderByDescending(n => n.SentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByStatusAsync(int userId, NotificationStatus status)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && n.Status == status)
                .OrderByDescending(n => n.SentDate)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && n.Status == NotificationStatus.Unread);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            notification.Status = NotificationStatus.Read;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.Status = NotificationStatus.Read;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteExpiredNotificationsAsync()
        {
            var expiredNotifications = await _context.Notifications
                .Where(n => n.ExpiryDate.HasValue && n.ExpiryDate.Value < DateTime.UtcNow)
                .ToListAsync();

            _context.Notifications.RemoveRange(expiredNotifications);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 