using System;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Dashboard
{
    public class RealTimeStatsResponse
    {
        public DateTime Timestamp { get; set; }
        public RealTimeMissionStats Missions { get; set; } = new RealTimeMissionStats();
        public RealTimeVehicleStats Vehicles { get; set; } = new RealTimeVehicleStats();
        public RealTimeDriverStats Drivers { get; set; } = new RealTimeDriverStats();
        public List<AlertItem> Alerts { get; set; } = new List<AlertItem>();
    }

    public class RealTimeMissionStats
    {
        public int Active { get; set; }
        public int CompletedToday { get; set; }
        public int Pending { get; set; }
    }

    public class RealTimeVehicleStats
    {
        public int Available { get; set; }
        public int InUse { get; set; }
    }

    public class RealTimeDriverStats
    {
        public int Available { get; set; }
        public int InTransit { get; set; }
    }

    public class AlertItem
    {
        public AlertType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public enum AlertType
    {
        Warning,
        Error,
        Info
    }
} 