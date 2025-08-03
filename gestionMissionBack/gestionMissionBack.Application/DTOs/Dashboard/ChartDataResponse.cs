using System;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Dashboard
{
    public class ChartDataResponse
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<ChartDataset> Datasets { get; set; } = new List<ChartDataset>();
    }

    public class ChartDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<double> Data { get; set; } = new List<double>();
        public object? BackgroundColor { get; set; } // Can be string or List<string>
        public string? BorderColor { get; set; }
        public int? BorderWidth { get; set; }
    }

    public class MissionStatusChartResponse : ChartDataResponse
    {
        // Inherits from ChartDataResponse
    }

    public class MissionTrendsChartResponse : ChartDataResponse
    {
        // Inherits from ChartDataResponse
    }

    public class VehicleAvailabilityChartResponse : ChartDataResponse
    {
        // Inherits from ChartDataResponse
    }

    public class DriverStatusChartResponse : ChartDataResponse
    {
        // Inherits from ChartDataResponse
    }

    public class UtilizationTrendsChartResponse : ChartDataResponse
    {
        // Inherits from ChartDataResponse
    }
} 