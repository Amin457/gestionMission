using System;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Dashboard
{
    public class VehicleStatsResponse
    {
        public VehicleSummary Summary { get; set; } = new VehicleSummary();
        public VehicleTypeStats ByType { get; set; } = new VehicleTypeStats();
        public VehicleUtilization Utilization { get; set; } = new VehicleUtilization();
        public VehicleCapacity Capacity { get; set; } = new VehicleCapacity();
    }

    public class VehicleSummary
    {
        public int Total { get; set; }
        public int Available { get; set; }
        public int InUse { get; set; }
        public int Maintenance { get; set; }
    }

    public class VehicleTypeStats
    {
        public VehicleTypeDetail Commercial { get; set; } = new VehicleTypeDetail();
        public VehicleTypeDetail Passenger { get; set; } = new VehicleTypeDetail();
        public VehicleTypeDetail Truck { get; set; } = new VehicleTypeDetail();
    }

    public class VehicleTypeDetail
    {
        public int Total { get; set; }
        public int Available { get; set; }
        public double Utilization { get; set; } // percentage
    }

    public class VehicleUtilization
    {
        public double AverageUtilization { get; set; } // percentage
        public double PeakUtilization { get; set; } // percentage
        public double LowUtilization { get; set; } // percentage
    }

    public class VehicleCapacity
    {
        public double TotalCapacity { get; set; }
        public double UsedCapacity { get; set; }
        public double AvailableCapacity { get; set; }
    }
} 