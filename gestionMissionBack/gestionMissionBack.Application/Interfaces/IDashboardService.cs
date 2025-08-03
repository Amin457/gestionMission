using System;
using System.Threading.Tasks;
using gestionMissionBack.Application.DTOs.Dashboard;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardOverviewResponse> GetOverviewAsync();
        
        Task<MissionStatsResponse> GetMissionStatsAsync(
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int? driverId = null,
            MissionType? type = null);
        
        Task<VehicleStatsResponse> GetVehicleStatsAsync();
        
        Task<DriverStatsResponse> GetDriverStatsAsync(
            DateTime? dateFrom = null,
            DateTime? dateTo = null);
        
        Task<ChartDataResponse> GetChartDataAsync(string chartType, DateTime? dateFrom = null, DateTime? dateTo = null);
        
        Task<RecentActivityResponse> GetRecentActivityAsync(int limit = 10);
        
        Task<FilteredMissionStatsResponse> GetFilteredMissionStatsAsync(DashboardFilter filter, string groupBy);
        
        Task<RealTimeStatsResponse> GetRealTimeStatsAsync();
    }
} 