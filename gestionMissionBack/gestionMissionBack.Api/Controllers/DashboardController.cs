using System;
using System.Threading.Tasks;
using gestionMissionBack.Application.DTOs.Dashboard;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        /// <summary>
        /// Get dashboard overview statistics
        /// </summary>
        /// <returns>Complete dashboard overview with missions, vehicles, drivers, and utilization stats</returns>
        [HttpGet("overview")]
        public async Task<ActionResult<DashboardOverviewResponse>> GetOverview()
        {
            try
            {
                var overview = await _dashboardService.GetOverviewAsync();
                return Ok(overview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving dashboard overview", error = ex.Message });
            }
        }

        /// <summary>
        /// Get detailed mission statistics with filtering options
        /// </summary>
        /// <param name="dateFrom">Start date for statistics (ISO string)</param>
        /// <param name="dateTo">End date for statistics (ISO string)</param>
        /// <param name="driverId">Filter by specific driver ID</param>
        /// <param name="type">Filter by mission type</param>
        /// <returns>Detailed mission statistics with trends and performance metrics</returns>
        [HttpGet("missions/stats")]
        public async Task<ActionResult<MissionStatsResponse>> GetMissionStats(
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] int? driverId = null,
            [FromQuery] MissionType? type = null)
        {
            try
            {
                var stats = await _dashboardService.GetMissionStatsAsync(dateFrom, dateTo, driverId, type);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving mission statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Get vehicle availability and utilization statistics
        /// </summary>
        /// <returns>Vehicle statistics including availability, type breakdown, and utilization rates</returns>
        [HttpGet("vehicles/stats")]
        public async Task<ActionResult<VehicleStatsResponse>> GetVehicleStats()
        {
            try
            {
                var stats = await _dashboardService.GetVehicleStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving vehicle statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Get driver performance and status statistics
        /// </summary>
        /// <param name="dateFrom">Start date for statistics (ISO string)</param>
        /// <param name="dateTo">End date for statistics (ISO string)</param>
        /// <returns>Driver statistics including performance metrics and status breakdown</returns>
        [HttpGet("drivers/stats")]
        public async Task<ActionResult<DriverStatsResponse>> GetDriverStats(
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            try
            {
                var stats = await _dashboardService.GetDriverStatsAsync(dateFrom, dateTo);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving driver statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Get specific chart data for dashboard visualizations
        /// </summary>
        /// <param name="chartType">Type of chart: mission-status, mission-type, vehicle-availability, driver-status, mission-trends, utilization-trends</param>
        /// <param name="dateFrom">Start date for chart data (ISO string)</param>
        /// <param name="dateTo">End date for chart data (ISO string)</param>
        /// <returns>Chart data formatted for frontend visualization libraries</returns>
        [HttpGet("charts/{chartType}")]
        public async Task<ActionResult<ChartDataResponse>> GetChartData(
            string chartType,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(chartType))
                {
                    return BadRequest(new { message = "Chart type is required" });
                }

                var chartData = await _dashboardService.GetChartDataAsync(chartType, dateFrom, dateTo);
                return Ok(chartData);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving chart data", error = ex.Message });
            }
        }

        /// <summary>
        /// Get recent dashboard activity for quick overview
        /// </summary>
        /// <param name="limit">Number of recent activities (default: 10)</param>
        /// <returns>Recent activities with summary information</returns>
        [HttpGet("recent-activity")]
        public async Task<ActionResult<RecentActivityResponse>> GetRecentActivity([FromQuery] int limit = 10)
        {
            try
            {
                if (limit <= 0 || limit > 100)
                {
                    return BadRequest(new { message = "Limit must be between 1 and 100" });
                }

                var activity = await _dashboardService.GetRecentActivityAsync(limit);
                return Ok(activity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving recent activity", error = ex.Message });
            }
        }

        /// <summary>
        /// Get filtered mission statistics based on complex filters
        /// </summary>
        /// <param name="filter">Complex filter criteria</param>
        /// <param name="groupBy">Grouping criteria: status, type, driver, date</param>
        /// <returns>Filtered mission statistics with grouped data and trends</returns>
        [HttpPost("missions/stats/filtered")]
        public async Task<ActionResult<FilteredMissionStatsResponse>> GetFilteredMissionStats(
            [FromBody] DashboardFilter filter,
            [FromQuery] string groupBy = "status")
        {
            try
            {
                if (filter == null)
                {
                    return BadRequest(new { message = "Filter criteria are required" });
                }

                if (string.IsNullOrWhiteSpace(groupBy))
                {
                    return BadRequest(new { message = "Group by parameter is required" });
                }

                var validGroupByOptions = new[] { "status", "type", "driver", "date" };
                if (!Array.Exists(validGroupByOptions, option => option.Equals(groupBy, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new { message = "Invalid group by option. Valid options are: status, type, driver, date" });
                }

                var stats = await _dashboardService.GetFilteredMissionStatsAsync(filter, groupBy);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving filtered mission statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Get real-time dashboard statistics for live updates
        /// </summary>
        /// <returns>Real-time statistics with current alerts</returns>
        [HttpGet("realtime")]
        public async Task<ActionResult<RealTimeStatsResponse>> GetRealTimeStats()
        {
            try
            {
                var realtimeStats = await _dashboardService.GetRealTimeStatsAsync();
                return Ok(realtimeStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving real-time statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available chart types
        /// </summary>
        /// <returns>List of available chart types for the dashboard</returns>
        [HttpGet("charts")]
        public ActionResult<object> GetAvailableChartTypes()
        {
            var chartTypes = new[]
            {
                new { Type = "mission-status", Description = "Mission distribution by status", Parameters = new[] { "dateFrom", "dateTo" } },
                new { Type = "mission-type", Description = "Mission distribution by type", Parameters = new[] { "dateFrom", "dateTo" } },
                new { Type = "vehicle-availability", Description = "Vehicle availability chart", Parameters = new string[] { } },
                new { Type = "driver-status", Description = "Driver status distribution", Parameters = new string[] { } },
                new { Type = "mission-trends", Description = "Mission trends over time", Parameters = new[] { "dateFrom", "dateTo" } },
                new { Type = "utilization-trends", Description = "Utilization trends over time", Parameters = new[] { "dateFrom", "dateTo" } }
            };

            return Ok(new { ChartTypes = chartTypes });
        }

        /// <summary>
        /// Get dashboard health status
        /// </summary>
        /// <returns>Dashboard service health information</returns>
        [HttpGet("health")]
        public ActionResult<object> GetHealth()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "Dashboard API",
                Version = "1.0.0"
            });
        }
    }
} 