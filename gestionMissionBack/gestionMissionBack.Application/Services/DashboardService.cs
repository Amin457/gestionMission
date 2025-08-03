using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gestionMissionBack.Application.DTOs.Dashboard;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace gestionMissionBack.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IMissionRepository _missionRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVehicleReservationRepository _vehicleReservationRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly IMissionCostRepository _missionCostRepository;

        public DashboardService(
            IMissionRepository missionRepository,
            IVehicleRepository vehicleRepository,
            IUserRepository userRepository,
            IVehicleReservationRepository vehicleReservationRepository,
            IIncidentRepository incidentRepository,
            IMissionCostRepository missionCostRepository)
        {
            _missionRepository = missionRepository;
            _vehicleRepository = vehicleRepository;
            _userRepository = userRepository;
            _vehicleReservationRepository = vehicleReservationRepository;
            _incidentRepository = incidentRepository;
            _missionCostRepository = missionCostRepository;
        }

        public async Task<DashboardOverviewResponse> GetOverviewAsync()
        {
            var missionsQuery = _missionRepository.GetQueryable();
            var vehiclesQuery = _vehicleRepository.GetQueryable();
            var usersQuery = _userRepository.GetQueryable();
            var reservationsQuery = _vehicleReservationRepository.GetQueryable();

            // Mission statistics
            var missionStats = new MissionStats
            {
                Total = await missionsQuery.CountAsync(),
                Active = await missionsQuery.CountAsync(m => m.Status == MissionStatus.InProgress),
                Completed = await missionsQuery.CountAsync(m => m.Status == MissionStatus.Completed),
                Pending = await missionsQuery.CountAsync(m => m.Status == MissionStatus.Requested || m.Status == MissionStatus.Approved),
                Rejected = await missionsQuery.CountAsync(m => m.Status == MissionStatus.Rejected)
            };

            // Vehicle statistics
            var vehicleStats = new VehicleStats
            {
                Total = await vehiclesQuery.CountAsync(),
                Available = await vehiclesQuery.CountAsync(v => v.Availability),
                InUse = await vehiclesQuery.CountAsync(v => !v.Availability)
            };

            // Driver statistics
            var driverStats = new DriverStats
            {
                Total = await usersQuery.CountAsync(u => u.RoleId == 2), // Assuming role 2 is drivers
                Active = await usersQuery.CountAsync(u => u.RoleId == 2 && u.CurrentDriverStatus != DriverStatus.OffDuty),
                Available = await usersQuery.CountAsync(u => u.RoleId == 2 && u.CurrentDriverStatus == DriverStatus.OffDuty),
                OffDuty = await usersQuery.CountAsync(u => u.RoleId == 2 && u.CurrentDriverStatus == DriverStatus.OffDuty)
            };

            // Utilization statistics
            var utilizationStats = new UtilizationStats
            {
                MissionCompletionRate = missionStats.Total > 0 ? (double)missionStats.Completed / missionStats.Total * 100 : 0,
                VehicleUtilizationRate = vehicleStats.Total > 0 ? (double)vehicleStats.InUse / vehicleStats.Total * 100 : 0,
                DriverUtilizationRate = driverStats.Total > 0 ? (double)driverStats.Active / driverStats.Total * 100 : 0
            };

            return new DashboardOverviewResponse
            {
                Missions = missionStats,
                Vehicles = vehicleStats,
                Drivers = driverStats,
                Utilization = utilizationStats
            };
        }

        public async Task<MissionStatsResponse> GetMissionStatsAsync(
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int? driverId = null,
            MissionType? type = null)
        {
            var query = _missionRepository.GetQueryable();

            // Apply filters
            if (dateFrom.HasValue)
                query = query.Where(m => m.SystemDate >= dateFrom.Value);
            if (dateTo.HasValue)
                query = query.Where(m => m.SystemDate <= dateTo.Value);
            if (driverId.HasValue)
                query = query.Where(m => m.DriverId == driverId.Value);
            if (type.HasValue)
                query = query.Where(m => m.Type == type.Value);

            var missions = await query.ToListAsync();

            var summary = new MissionSummary
            {
                Total = missions.Count,
                ByStatus = new MissionStatusCounts
                {
                    Requested = missions.Count(m => m.Status == MissionStatus.Requested),
                    Approved = missions.Count(m => m.Status == MissionStatus.Approved),
                    Planned = missions.Count(m => m.Status == MissionStatus.Planned),
                    InProgress = missions.Count(m => m.Status == MissionStatus.InProgress),
                    Completed = missions.Count(m => m.Status == MissionStatus.Completed),
                    Rejected = missions.Count(m => m.Status == MissionStatus.Rejected)
                },
                ByType = new MissionTypeCounts
                {
                    Goods = missions.Count(m => m.Type == MissionType.Goods),
                    Financial = missions.Count(m => m.Type == MissionType.Financial),
                    Administrative = missions.Count(m => m.Type == MissionType.Administrative)
                }
            };

            // Calculate trends (last 30 days)
            var trends = await CalculateMissionTrendsAsync(dateFrom, dateTo, driverId, type);

            // Calculate performance metrics
            var performance = new MissionPerformance
            {
                CompletionRate = summary.Total > 0 ? (double)summary.ByStatus.Completed / summary.Total * 100 : 0,
                AverageCompletionTime = CalculateAverageCompletionTime(missions),
                OnTimeDeliveryRate = CalculateOnTimeDeliveryRate(missions)
            };

            return new MissionStatsResponse
            {
                Summary = summary,
                Trends = trends,
                Performance = performance
            };
        }

        public async Task<VehicleStatsResponse> GetVehicleStatsAsync()
        {
            var vehicles = await _vehicleRepository.GetQueryable().ToListAsync();
            var reservations = await _vehicleReservationRepository.GetQueryable().ToListAsync();

            var summary = new VehicleSummary
            {
                Total = vehicles.Count,
                Available = vehicles.Count(v => v.Availability),
                InUse = vehicles.Count(v => !v.Availability),
                Maintenance = 0 // Could be calculated based on maintenance records
            };

            var byType = new VehicleTypeStats
            {
                Commercial = CalculateVehicleTypeStats(vehicles, VehicleType.Commercial),
                Passenger = CalculateVehicleTypeStats(vehicles, VehicleType.Passenger),
                Truck = CalculateVehicleTypeStats(vehicles, VehicleType.Truck)
            };

            var utilization = new VehicleUtilization
            {
                AverageUtilization = summary.Total > 0 ? (double)summary.InUse / summary.Total * 100 : 0,
                PeakUtilization = CalculatePeakUtilization(vehicles, reservations),
                LowUtilization = CalculateLowUtilization(vehicles, reservations)
            };

            var capacity = new VehicleCapacity
            {
                TotalCapacity = vehicles.Sum(v => v.MaxCapacity),
                UsedCapacity = vehicles.Where(v => !v.Availability).Sum(v => v.MaxCapacity),
                AvailableCapacity = vehicles.Where(v => v.Availability).Sum(v => v.MaxCapacity)
            };

            return new VehicleStatsResponse
            {
                Summary = summary,
                ByType = byType,
                Utilization = utilization,
                Capacity = capacity
            };
        }

        public async Task<DriverStatsResponse> GetDriverStatsAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var drivers = await _userRepository.GetQueryable()
                .Where(u => u.RoleId == 2) // Assuming role 2 is drivers
                .ToListAsync();

            var missions = await _missionRepository.GetQueryable()
                .Where(m => (!dateFrom.HasValue || m.SystemDate >= dateFrom.Value) &&
                           (!dateTo.HasValue || m.SystemDate <= dateTo.Value))
                .ToListAsync();

            var summary = new DriverSummary
            {
                Total = drivers.Count,
                Active = drivers.Count(d => d.CurrentDriverStatus != DriverStatus.OffDuty),
                Available = drivers.Count(d => d.CurrentDriverStatus == DriverStatus.OffDuty),
                OffDuty = drivers.Count(d => d.CurrentDriverStatus == DriverStatus.OffDuty),
                OnBreak = drivers.Count(d => d.CurrentDriverStatus == DriverStatus.OnBreak)
            };

            var performance = new DriverPerformance
            {
                TopPerformers = await GetTopPerformersAsync(drivers, missions),
                AverageMetrics = CalculateAverageMetrics(drivers, missions)
            };

            var status = new DriverStatusBreakdown
            {
                ByStatus = new DriverStatusCounts
                {
                    Available = drivers.Count(d => d.CurrentDriverStatus == DriverStatus.OffDuty),
                    InTransit = drivers.Count(d => d.CurrentDriverStatus == DriverStatus.InTransit),
                    OffDuty = drivers.Count(d => d.CurrentDriverStatus == DriverStatus.OffDuty),
                    OnBreak = drivers.Count(d => d.CurrentDriverStatus == DriverStatus.OnBreak)
                },
                AvailabilityTrend = await CalculateAvailabilityTrendAsync(drivers, dateFrom, dateTo)
            };

            return new DriverStatsResponse
            {
                Summary = summary,
                Performance = performance,
                Status = status
            };
        }

        public async Task<ChartDataResponse> GetChartDataAsync(string chartType, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            return chartType.ToLower() switch
            {
                "mission-status" => await GetMissionStatusChartAsync(),
                "mission-type" => await GetMissionTypeChartAsync(),
                "vehicle-availability" => await GetVehicleAvailabilityChartAsync(),
                "driver-status" => await GetDriverStatusChartAsync(),
                "mission-trends" => await GetMissionTrendsChartAsync(dateFrom, dateTo),
                "utilization-trends" => await GetUtilizationTrendsChartAsync(dateFrom, dateTo),
                _ => throw new ArgumentException($"Unknown chart type: {chartType}")
            };
        }

        public async Task<RecentActivityResponse> GetRecentActivityAsync(int limit = 10)
        {
            var activities = new List<ActivityItem>();

            // Get recent missions
            var recentMissions = await _missionRepository.GetQueryable()
                .OrderByDescending(m => m.SystemDate)
                .Take(limit / 2)
                .ToListAsync();

            foreach (var mission in recentMissions)
            {
                activities.Add(new ActivityItem
                {
                    Id = mission.MissionId,
                    Type = ActivityType.MissionCreated,
                    Title = $"New Mission Created",
                    Description = $"Mission {mission.MissionId} - {mission.Service}",
                    Timestamp = mission.SystemDate,
                    RelatedId = mission.MissionId
                });
            }

            // Get recent completed missions
            var completedMissions = await _missionRepository.GetQueryable()
                .Where(m => m.Status == MissionStatus.Completed)
                .OrderByDescending(m => m.SystemDate)
                .Take(limit / 2)
                .ToListAsync();

            foreach (var mission in completedMissions)
            {
                activities.Add(new ActivityItem
                {
                    Id = mission.MissionId,
                    Type = ActivityType.MissionCompleted,
                    Title = $"Mission Completed",
                    Description = $"Mission {mission.MissionId} completed successfully",
                    Timestamp = mission.SystemDate,
                    RelatedId = mission.MissionId
                });
            }

            var summary = new ActivitySummary
            {
                NewMissions = recentMissions.Count,
                CompletedMissions = completedMissions.Count,
                StatusChanges = 0 // Could be calculated from audit logs
            };

            return new RecentActivityResponse
            {
                Activities = activities.OrderByDescending(a => a.Timestamp).Take(limit).ToList(),
                Summary = summary
            };
        }

        public async Task<FilteredMissionStatsResponse> GetFilteredMissionStatsAsync(DashboardFilter filter, string groupBy)
        {
            var query = _missionRepository.GetQueryable();

            // Apply filters
            if (filter.DateRange != null)
            {
                query = query.Where(m => m.SystemDate >= filter.DateRange.From && m.SystemDate <= filter.DateRange.To);
            }

            if (filter.Statuses != null && filter.Statuses.Any())
            {
                query = query.Where(m => filter.Statuses.Contains(m.Status));
            }

            if (filter.Types != null && filter.Types.Any())
            {
                query = query.Where(m => filter.Types.Contains(m.Type));
            }

            if (filter.DriverIds != null && filter.DriverIds.Any())
            {
                query = query.Where(m => filter.DriverIds.Contains(m.DriverId));
            }

            var missions = await query.ToListAsync();

            var groupedData = groupBy.ToLower() switch
            {
                "status" => missions.GroupBy(m => m.Status)
                    .Select(g => new GroupedData { Group = g.Key.ToString(), Count = g.Count(), Percentage = (double)g.Count() / missions.Count * 100 })
                    .ToList(),
                "type" => missions.GroupBy(m => m.Type)
                    .Select(g => new GroupedData { Group = g.Key.ToString(), Count = g.Count(), Percentage = (double)g.Count() / missions.Count * 100 })
                    .ToList(),
                "driver" => missions.GroupBy(m => m.DriverId)
                    .Select(g => new GroupedData { Group = $"Driver {g.Key}", Count = g.Count(), Percentage = (double)g.Count() / missions.Count * 100 })
                    .ToList(),
                "date" => missions.GroupBy(m => m.SystemDate.Date)
                    .Select(g => new GroupedData { Group = g.Key.ToString("yyyy-MM-dd"), Count = g.Count(), Percentage = (double)g.Count() / missions.Count * 100 })
                    .ToList(),
                _ => new List<GroupedData>()
            };

            var trends = missions.GroupBy(m => m.SystemDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new TrendData { Period = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
                .ToList();

            return new FilteredMissionStatsResponse
            {
                Summary = new FilteredSummary { Total = await _missionRepository.GetQueryable().CountAsync(), Filtered = missions.Count },
                GroupedData = groupedData,
                Trends = trends
            };
        }

        public async Task<RealTimeStatsResponse> GetRealTimeStatsAsync()
        {
            var now = DateTime.Now;
            var today = now.Date;

            var missions = await _missionRepository.GetQueryable().ToListAsync();
            var vehicles = await _vehicleRepository.GetQueryable().ToListAsync();
            var drivers = await _userRepository.GetQueryable().Where(u => u.RoleId == 2).ToListAsync();

            var alerts = new List<AlertItem>();

            // Check for low vehicle availability
            var availableVehicles = vehicles.Count(v => v.Availability);
            if (availableVehicles < vehicles.Count * 0.2) // Less than 20% available
            {
                alerts.Add(new AlertItem
                {
                    Type = AlertType.Warning,
                    Message = $"Low vehicle availability: {availableVehicles} out of {vehicles.Count} vehicles available",
                    Timestamp = now
                });
            }

            // Check for pending missions
            var pendingMissions = missions.Count(m => m.Status == MissionStatus.Requested || m.Status == MissionStatus.Approved);
            if (pendingMissions > 10)
            {
                alerts.Add(new AlertItem
                {
                    Type = AlertType.Info,
                    Message = $"{pendingMissions} missions pending approval",
                    Timestamp = now
                });
            }

            return new RealTimeStatsResponse
            {
                Timestamp = now,
                Missions = new RealTimeMissionStats
                {
                    Active = missions.Count(m => m.Status == MissionStatus.InProgress),
                    CompletedToday = missions.Count(m => m.Status == MissionStatus.Completed && m.SystemDate.Date == today),
                    Pending = pendingMissions
                },
                Vehicles = new RealTimeVehicleStats
                {
                    Available = availableVehicles,
                    InUse = vehicles.Count(v => !v.Availability)
                },
                Drivers = new RealTimeDriverStats
                {
                    Available = drivers.Count(d => d.CurrentDriverStatus == DriverStatus.OffDuty),
                    InTransit = drivers.Count(d => d.CurrentDriverStatus == DriverStatus.InTransit)
                },
                Alerts = alerts
            };
        }

        #region Private Helper Methods

        private async Task<MissionTrends> CalculateMissionTrendsAsync(DateTime? dateFrom, DateTime? dateTo, int? driverId, MissionType? type)
        {
            var query = _missionRepository.GetQueryable();

            if (dateFrom.HasValue)
                query = query.Where(m => m.SystemDate >= dateFrom.Value);
            if (dateTo.HasValue)
                query = query.Where(m => m.SystemDate <= dateTo.Value);
            if (driverId.HasValue)
                query = query.Where(m => m.DriverId == driverId.Value);
            if (type.HasValue)
                query = query.Where(m => m.Type == type.Value);

            var missions = await query.ToListAsync();

            var daily = missions.GroupBy(m => m.SystemDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DailyTrend
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count(),
                    Completed = g.Count(m => m.Status == MissionStatus.Completed)
                })
                .ToList();

            var weekly = missions.GroupBy(m => GetWeekOfYear(m.SystemDate))
                .OrderBy(g => g.Key)
                .Select(g => new WeeklyTrend
                {
                    Week = g.Key,
                    Count = g.Count(),
                    Completed = g.Count(m => m.Status == MissionStatus.Completed)
                })
                .ToList();

            var monthly = missions.GroupBy(m => new { m.SystemDate.Year, m.SystemDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyTrend
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count(),
                    Completed = g.Count(m => m.Status == MissionStatus.Completed)
                })
                .ToList();

            return new MissionTrends { Daily = daily, Weekly = weekly, Monthly = monthly };
        }

        private string GetWeekOfYear(DateTime date)
        {
            var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            var week = calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return $"{date.Year}-W{week:D2}";
        }

        private double CalculateAverageCompletionTime(List<Domain.Entities.Mission> missions)
        {
            var completedMissions = missions.Where(m => m.Status == MissionStatus.Completed).ToList();
            if (!completedMissions.Any()) return 0;

            // This is a simplified calculation - in a real scenario you'd have completion timestamps
            return 24.0; // Placeholder: average 24 hours
        }

        private double CalculateOnTimeDeliveryRate(List<Domain.Entities.Mission> missions)
        {
            var completedMissions = missions.Where(m => m.Status == MissionStatus.Completed).ToList();
            if (!completedMissions.Any()) return 0;

            // This is a simplified calculation - in a real scenario you'd compare desired vs actual dates
            return 85.0; // Placeholder: 85% on-time delivery
        }

        private VehicleTypeDetail CalculateVehicleTypeStats(List<Domain.Entities.Vehicle> vehicles, VehicleType type)
        {
            var typeVehicles = vehicles.Where(v => v.Type == type).ToList();
            var total = typeVehicles.Count;
            var available = typeVehicles.Count(v => v.Availability);
            var utilization = total > 0 ? (double)(total - available) / total * 100 : 0;

            return new VehicleTypeDetail
            {
                Total = total,
                Available = available,
                Utilization = utilization
            };
        }

        private double CalculatePeakUtilization(List<Domain.Entities.Vehicle> vehicles, List<Domain.Entities.VehicleReservation> reservations)
        {
            // Simplified calculation - in reality you'd analyze historical data
            return 75.0; // Placeholder: 75% peak utilization
        }

        private double CalculateLowUtilization(List<Domain.Entities.Vehicle> vehicles, List<Domain.Entities.VehicleReservation> reservations)
        {
            // Simplified calculation - in reality you'd analyze historical data
            return 25.0; // Placeholder: 25% low utilization
        }

        private async Task<List<TopPerformer>> GetTopPerformersAsync(List<Domain.Entities.User> drivers, List<Domain.Entities.Mission> missions)
        {
            var topPerformers = new List<TopPerformer>();

            foreach (var driver in drivers.Take(5)) // Top 5 drivers
            {
                var driverMissions = missions.Where(m => m.DriverId == driver.UserId).ToList();
                var completedMissions = driverMissions.Where(m => m.Status == MissionStatus.Completed).Count();

                topPerformers.Add(new TopPerformer
                {
                    DriverId = driver.UserId,
                    DriverName = $"{driver.FirstName} {driver.LastName}",
                    MissionsCompleted = completedMissions,
                    AverageRating = 4.5, // Placeholder - would come from rating system
                    TotalDistance = 1500.0 // Placeholder - would be calculated from actual routes
                });
            }

            return topPerformers.OrderByDescending(p => p.MissionsCompleted).ToList();
        }

        private AverageMetrics CalculateAverageMetrics(List<Domain.Entities.User> drivers, List<Domain.Entities.Mission> missions)
        {
            var totalDrivers = drivers.Count;
            if (totalDrivers == 0) return new AverageMetrics();

            var totalMissions = missions.Count;
            var completedMissions = missions.Where(m => m.Status == MissionStatus.Completed).Count();

            return new AverageMetrics
            {
                MissionsPerDriver = totalDrivers > 0 ? (double)totalMissions / totalDrivers : 0,
                AverageRating = 4.2, // Placeholder
                AverageDistance = 1200.0 // Placeholder
            };
        }

        private async Task<List<AvailabilityTrend>> CalculateAvailabilityTrendAsync(List<Domain.Entities.User> drivers, DateTime? dateFrom, DateTime? dateTo)
        {
            // Simplified calculation - in reality you'd analyze historical availability data
            var trends = new List<AvailabilityTrend>();
            var startDate = dateFrom ?? DateTime.Now.AddDays(-30);
            var endDate = dateTo ?? DateTime.Now;

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                trends.Add(new AvailabilityTrend
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Available = drivers.Count(d => d.CurrentDriverStatus == DriverStatus.OffDuty),
                    Total = drivers.Count
                });
            }

            return trends;
        }

        private async Task<ChartDataResponse> GetMissionStatusChartAsync()
        {
            var missions = await _missionRepository.GetQueryable().ToListAsync();
            var statusCounts = missions.GroupBy(m => m.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToList();

            return new ChartDataResponse
            {
                Labels = statusCounts.Select(s => s.Status).ToList(),
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Missions by Status",
                        Data = statusCounts.Select(s => (double)s.Count).ToList(),
                        BackgroundColor = new List<string> { "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF", "#FF9F40" }
                    }
                }
            };
        }

        private async Task<ChartDataResponse> GetMissionTypeChartAsync()
        {
            var missions = await _missionRepository.GetQueryable().ToListAsync();
            var typeCounts = missions.GroupBy(m => m.Type)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToList();

            return new ChartDataResponse
            {
                Labels = typeCounts.Select(t => t.Type).ToList(),
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Missions by Type",
                        Data = typeCounts.Select(t => (double)t.Count).ToList(),
                        BackgroundColor = new List<string> { "#FF6384", "#36A2EB", "#FFCE56" }
                    }
                }
            };
        }

        private async Task<ChartDataResponse> GetVehicleAvailabilityChartAsync()
        {
            var vehicles = await _vehicleRepository.GetQueryable().ToListAsync();
            var available = vehicles.Count(v => v.Availability);
            var inUse = vehicles.Count(v => !v.Availability);

            return new ChartDataResponse
            {
                Labels = new List<string> { "Available", "In Use" },
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Vehicle Availability",
                        Data = new List<double> { (double)available, (double)inUse },
                        BackgroundColor = new List<string> { "#4BC0C0", "#FF6384" }
                    }
                }
            };
        }

        private async Task<ChartDataResponse> GetDriverStatusChartAsync()
        {
            var drivers = await _userRepository.GetQueryable()
                .Where(u => u.RoleId == 2)
                .ToListAsync();

            var statusCounts = drivers.GroupBy(d => d.CurrentDriverStatus)
                .Select(g => new { Status = g.Key?.ToString() ?? "Unknown", Count = g.Count() })
                .ToList();

            return new ChartDataResponse
            {
                Labels = statusCounts.Select(s => s.Status).ToList(),
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Driver Status",
                        Data = statusCounts.Select(s => (double)s.Count).ToList(),
                        BackgroundColor = new List<string> { "#36A2EB", "#FFCE56", "#FF6384", "#4BC0C0" }
                    }
                }
            };
        }

        private async Task<ChartDataResponse> GetMissionTrendsChartAsync(DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _missionRepository.GetQueryable();
            if (dateFrom.HasValue) query = query.Where(m => m.SystemDate >= dateFrom.Value);
            if (dateTo.HasValue) query = query.Where(m => m.SystemDate <= dateTo.Value);

            var missions = await query.ToListAsync();
            var dailyTrends = missions.GroupBy(m => m.SystemDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key.ToString("MM-dd"), Count = g.Count() })
                .ToList();

            return new ChartDataResponse
            {
                Labels = dailyTrends.Select(t => t.Date).ToList(),
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Missions per Day",
                        Data = dailyTrends.Select(t => (double)t.Count).ToList(),
                        BorderColor = "#36A2EB",
                        BackgroundColor = "rgba(54, 162, 235, 0.2)"
                    }
                }
            };
        }

        private async Task<ChartDataResponse> GetUtilizationTrendsChartAsync(DateTime? dateFrom, DateTime? dateTo)
        {
            // Simplified utilization trends - in reality you'd calculate based on actual usage data
            var labels = new List<string>();
            var vehicleData = new List<double>();
            var driverData = new List<double>();

            var startDate = dateFrom ?? DateTime.Now.AddDays(-30);
            var endDate = dateTo ?? DateTime.Now;

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(7))
            {
                labels.Add(date.ToString("MM-dd"));
                vehicleData.Add(65.0 + new Random().Next(-10, 10)); // Simulated data
                driverData.Add(75.0 + new Random().Next(-15, 15)); // Simulated data
            }

            return new ChartDataResponse
            {
                Labels = labels,
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Vehicle Utilization",
                        Data = vehicleData,
                        BorderColor = "#FF6384",
                        BackgroundColor = "rgba(255, 99, 132, 0.2)"
                    },
                    new ChartDataset
                    {
                        Label = "Driver Utilization",
                        Data = driverData,
                        BorderColor = "#36A2EB",
                        BackgroundColor = "rgba(54, 162, 235, 0.2)"
                    }
                }
            };
        }

        #endregion
    }
} 