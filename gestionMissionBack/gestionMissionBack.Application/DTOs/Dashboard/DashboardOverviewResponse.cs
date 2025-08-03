using System;

namespace gestionMissionBack.Application.DTOs.Dashboard
{
    public class DashboardOverviewResponse
    {
        public MissionStats Missions { get; set; } = new MissionStats();
        public VehicleStats Vehicles { get; set; } = new VehicleStats();
        public DriverStats Drivers { get; set; } = new DriverStats();
        public UtilizationStats Utilization { get; set; } = new UtilizationStats();
    }

    public class MissionStats
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Completed { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
    }

    public class VehicleStats
    {
        public int Total { get; set; }
        public int Available { get; set; }
        public int InUse { get; set; }
    }

    public class DriverStats
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Available { get; set; }
        public int OffDuty { get; set; }
    }

    public class UtilizationStats
    {
        public double MissionCompletionRate { get; set; } // Percentage
        public double VehicleUtilizationRate { get; set; } // Percentage
        public double DriverUtilizationRate { get; set; } // Percentage
    }
} 