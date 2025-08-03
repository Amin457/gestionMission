using System;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Dashboard
{
    public class DriverStatsResponse
    {
        public DriverSummary Summary { get; set; } = new DriverSummary();
        public DriverPerformance Performance { get; set; } = new DriverPerformance();
        public DriverStatusBreakdown Status { get; set; } = new DriverStatusBreakdown();
    }

    public class DriverSummary
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Available { get; set; }
        public int OffDuty { get; set; }
        public int OnBreak { get; set; }
    }

    public class DriverPerformance
    {
        public List<TopPerformer> TopPerformers { get; set; } = new List<TopPerformer>();
        public AverageMetrics AverageMetrics { get; set; } = new AverageMetrics();
    }

    public class TopPerformer
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public int MissionsCompleted { get; set; }
        public double AverageRating { get; set; }
        public double TotalDistance { get; set; }
    }

    public class AverageMetrics
    {
        public double MissionsPerDriver { get; set; }
        public double AverageRating { get; set; }
        public double AverageDistance { get; set; }
    }

    public class DriverStatusBreakdown
    {
        public DriverStatusCounts ByStatus { get; set; } = new DriverStatusCounts();
        public List<AvailabilityTrend> AvailabilityTrend { get; set; } = new List<AvailabilityTrend>();
    }

    public class DriverStatusCounts
    {
        public int Available { get; set; }
        public int InTransit { get; set; }
        public int OffDuty { get; set; }
        public int OnBreak { get; set; }
    }

    public class AvailabilityTrend
    {
        public string Date { get; set; } = string.Empty;
        public int Available { get; set; }
        public int Total { get; set; }
    }
} 