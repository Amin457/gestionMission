using gestionMissionBack.Domain.Enums;
using System;

namespace gestionMissionBack.Domain.Helpers
{
    public class NotificationFilter
    {
        public NotificationStatus? Status { get; set; }
        public NotificationCategory? Type { get; set; }
        public NotificationPriority? Priority { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
    }
} 