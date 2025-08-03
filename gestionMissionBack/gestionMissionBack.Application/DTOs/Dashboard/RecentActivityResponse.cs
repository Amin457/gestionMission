using System;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Dashboard
{
    public class RecentActivityResponse
    {
        public List<ActivityItem> Activities { get; set; } = new List<ActivityItem>();
        public ActivitySummary Summary { get; set; } = new ActivitySummary();
    }

    public class ActivityItem
    {
        public int Id { get; set; }
        public ActivityType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public int? RelatedId { get; set; } // missionId, vehicleId, etc.
    }

    public class ActivitySummary
    {
        public int NewMissions { get; set; }
        public int CompletedMissions { get; set; }
        public int StatusChanges { get; set; }
    }

    public enum ActivityType
    {
        MissionCreated,
        MissionCompleted,
        VehicleAssigned,
        DriverStatusChanged,
        IncidentReported,
        MissionCostAdded
    }
} 