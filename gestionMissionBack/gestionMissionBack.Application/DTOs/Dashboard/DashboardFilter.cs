using System;
using System.Collections.Generic;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.DTOs.Dashboard
{
    public class DashboardFilter
    {
        public DateRange? DateRange { get; set; }
        public List<MissionStatus>? Statuses { get; set; }
        public List<MissionType>? Types { get; set; }
        public List<int>? DriverIds { get; set; }
        public List<string>? Requesters { get; set; }
        public List<VehicleType>? VehicleTypes { get; set; }
    }

    public class DateRange
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class FilteredMissionStatsResponse
    {
        public FilteredSummary Summary { get; set; } = new FilteredSummary();
        public List<GroupedData> GroupedData { get; set; } = new List<GroupedData>();
        public List<TrendData> Trends { get; set; } = new List<TrendData>();
    }

    public class FilteredSummary
    {
        public int Total { get; set; }
        public int Filtered { get; set; }
    }

    public class GroupedData
    {
        public string Group { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class TrendData
    {
        public string Period { get; set; } = string.Empty;
        public int Count { get; set; }
    }
} 