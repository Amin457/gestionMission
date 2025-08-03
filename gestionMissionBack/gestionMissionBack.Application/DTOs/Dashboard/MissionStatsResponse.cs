using System;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Dashboard
{
    public class MissionStatsResponse
    {
        public MissionSummary Summary { get; set; } = new MissionSummary();
        public MissionTrends Trends { get; set; } = new MissionTrends();
        public MissionPerformance Performance { get; set; } = new MissionPerformance();
    }

    public class MissionSummary
    {
        public int Total { get; set; }
        public MissionStatusCounts ByStatus { get; set; } = new MissionStatusCounts();
        public MissionTypeCounts ByType { get; set; } = new MissionTypeCounts();
    }

    public class MissionStatusCounts
    {
        public int Requested { get; set; }
        public int Approved { get; set; }
        public int Planned { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Rejected { get; set; }
    }

    public class MissionTypeCounts
    {
        public int Goods { get; set; }
        public int Financial { get; set; }
        public int Administrative { get; set; }
    }

    public class MissionTrends
    {
        public List<DailyTrend> Daily { get; set; } = new List<DailyTrend>();
        public List<WeeklyTrend> Weekly { get; set; } = new List<WeeklyTrend>();
        public List<MonthlyTrend> Monthly { get; set; } = new List<MonthlyTrend>();
    }

    public class DailyTrend
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Completed { get; set; }
    }

    public class WeeklyTrend
    {
        public string Week { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Completed { get; set; }
    }

    public class MonthlyTrend
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Completed { get; set; }
    }

    public class MissionPerformance
    {
        public double AverageCompletionTime { get; set; } // in hours
        public double CompletionRate { get; set; } // percentage
        public double OnTimeDeliveryRate { get; set; } // percentage
    }
} 