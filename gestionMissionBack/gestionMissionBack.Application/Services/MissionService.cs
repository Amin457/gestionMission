using AutoMapper;
using gestionMissionBack.Application.DTOs.Mission;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Headers;
using gestionMissionBack.Application.DTOs;
using gestionMissionBack.Application.DTOs.Circuit;
using gestionMissionBack.Application.DTOs.Route;
using gestionMissionBack.Application.DTOs.Notification;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.Services
{
    public class MissionService : IMissionService
    {
        private readonly IMissionRepository _missionRepository;
        private readonly ICircuitService _circuitService;
        private readonly IRouteService _routeService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly string _openRouteApiKey;

        public MissionService(
            IMissionRepository missionRepository,
            ICircuitService circuitService,
            IRouteService routeService,
            INotificationService notificationService,
            IMapper mapper,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _missionRepository = missionRepository;
            _circuitService = circuitService;
            _routeService = routeService;
            _notificationService = notificationService;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient("OpenRouteService");
            _openRouteApiKey = configuration["OpenRouteService:ApiKey"] 
                ?? throw new ArgumentNullException("OpenRouteService:ApiKey configuration is missing");
        }

        public async Task<MissionDto> GetMissionByIdAsync(int id)
        {
            var mission = await _missionRepository.GetByIdAsync(id);
            return _mapper.Map<MissionDto>(mission);
        }

        public async Task<MissionDto> CreateMissionAsync(MissionDto missionDto)
        {
            var mission = _mapper.Map<Mission>(missionDto);
            mission.SystemDate = DateTime.Now;
            var newMission = await _missionRepository.AddAsync(mission);
            
            // Automatic notification to driver when mission is created
            await _notificationService.SendRealTimeNotificationAsync(
                newMission.DriverId,
                new CreateNotificationDto
                {
                    UserId = newMission.DriverId,
                    Title = "New Mission Assigned",
                    Message = $"You have been assigned mission #{newMission.Service} to {newMission.Receiver}",
                    NotificationType = NotificationCategory.Mission,
                    Priority = NotificationPriority.High,
                    RelatedEntityType = "Mission",
                    RelatedEntityId = newMission.MissionId
                }
            );
            
            return _mapper.Map<MissionDto>(newMission);
        }

        public async Task<bool> UpdateMissionAsync(MissionDto missionDto)
        {
            // Get the original mission to check status changes
            var originalMission = await _missionRepository.GetByIdAsync(missionDto.MissionId);
            if (originalMission == null) return false;
            
            var originalStatus = originalMission.Status;
            
            // Update the original mission entity instead of creating a new one
            _mapper.Map(missionDto, originalMission);
            var success = await _missionRepository.UpdateAsync(originalMission);
            
            // Automatic notification if status changed
            if (success && originalStatus != originalMission.Status)
            {
                // Notify driver
                await _notificationService.SendRealTimeNotificationAsync(
                    originalMission.DriverId,
                    new CreateNotificationDto
                    {
                        UserId = originalMission.DriverId,
                        Title = "Mission Status Updated",
                        Message = $"Mission #{originalMission.Service} status changed to {originalMission.Status}",
                        NotificationType = NotificationCategory.Mission,
                        Priority = NotificationPriority.Normal,
                        RelatedEntityType = "Mission",
                        RelatedEntityId = originalMission.MissionId
                    }
                );
                
                // Notify requester
                await _notificationService.SendRealTimeNotificationAsync(
                    originalMission.RequesterId,
                    new CreateNotificationDto
                    {
                        UserId = originalMission.RequesterId,
                        Title = "Mission Status Updated",
                        Message = $"Your mission #{originalMission.Service} status changed to {originalMission.Status}",
                        NotificationType = NotificationCategory.Mission,
                        Priority = NotificationPriority.Normal,
                        RelatedEntityType = "Mission",
                        RelatedEntityId = originalMission.MissionId
                    }
                );
            }
            
            return success;
        }

        public async Task<PagedResult<MissionDtoGet>> GetPagedAsync(int pageNumber, int pageSize, MissionFilter filters = null)
        {
            var query = _missionRepository.GetQueryable();

            if (filters != null)
            {
                if (filters.Type.HasValue)
                    query = query.Where(m => m.Type == filters.Type.Value);
                if (filters.Status.HasValue)
                    query = query.Where(m => m.Status == filters.Status.Value);
                if (filters.DriverId.HasValue)
                    query = query.Where(m => m.DriverId == filters.DriverId.Value);
                if (filters.DesiredDateStart.HasValue)
                    query = query.Where(m => m.DesiredDate >= filters.DesiredDateStart.Value);
                if (filters.DesiredDateEnd.HasValue)
                    query = query.Where(m => m.DesiredDate <= filters.DesiredDateEnd.Value);
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .Include(m => m.Requester)
                .Include(m => m.Driver)
                .Include(m => m.Tasks)
                    .ThenInclude(t => t.Site)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<MissionDtoGet>
            {
                Data = _mapper.Map<IEnumerable<MissionDtoGet>>(data),
                TotalRecords = totalRecords
            };
        }

        public async Task<bool> DeleteMissionAsync(int id)
        {
            return await _missionRepository.DeleteAsync(id);
        }

        public async Task<bool> GenerateCircuitsForMissionAsync(int missionId)
        {
            var mission = await _missionRepository.GetByIdAsync(missionId);
            if (mission == null) return false;

            // Check for existing circuits
            var existingCircuits = await _circuitService.GetCircuitsByMissionIdAsync(missionId);
            if (existingCircuits != null && existingCircuits.Any())
            {
                // Delete all existing circuits and their routes
                foreach (var existingCircuit in existingCircuits)
                {
                    await _routeService.DeleteRoutesByCircuitIdAsync(existingCircuit.CircuitId);
                    await _circuitService.DeleteCircuitAsync(existingCircuit.CircuitId);
                }
            }

            // Get all tasks with their sites
            var tasks = await _missionRepository.GetQueryable()
                .Where(m => m.MissionId == missionId)
                .Include(m => m.Tasks)
                    .ThenInclude(t => t.Site)
                .SelectMany(m => m.Tasks)
                .ToListAsync();

            if (!tasks.Any()) return false;

            // Find the first task
            var firstTask = tasks.FirstOrDefault(t => t.IsFirstTask);
            if (firstTask == null)
            {
                // If no task is marked as first, use the first task in the list
                firstTask = tasks.First();
                firstTask.IsFirstTask = true;
                await _missionRepository.UpdateAsync(mission);
            }

            // Extract locations from sites - OpenRouteService expects [longitude, latitude]
            var locations = tasks.Select(t => new List<double> 
            { 
                (double)t.Site.Longitude, 
                (double)t.Site.Latitude 
            }).ToList();

            // Call OpenRouteService API
            var request = new OpenRouteMatrixRequest 
            { 
                Locations = locations,
                Metrics = new List<string> { "distance", "duration" },
                Units = "m",
                Profile = "driving-car",
                Sources = new List<string> { "all" },
                Destinations = new List<string> { "all" }
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", _openRouteApiKey);

            var response = await _httpClient.PostAsync("https://api.openrouteservice.org/v2/matrix/driving-car", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"OpenRouteService API Error: {response.StatusCode} - {errorContent}");
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var matrixResponse = JsonSerializer.Deserialize<OpenRouteMatrixResponse>(
                responseContent, 
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );

            // Implement TSP (Traveling Salesman Problem) algorithm starting from the first task
            var optimalRoute = FindOptimalRouteFromStart(matrixResponse.Distances, tasks.IndexOf(firstTask));

            // Create circuit
            var newCircuit = new CircuitDto
            {
                MissionId = missionId,
                DepartureDate = DateTime.Now,
                DepartureSiteId = firstTask.Site.SiteId,
                ArrivalSiteId = tasks[optimalRoute[optimalRoute.Count - 1]].Site.SiteId,
                DepartureSiteName = firstTask.Site.Name,
                ArrivalSiteName = tasks[optimalRoute[optimalRoute.Count - 1]].Site.Name
            };

            // Create the circuit first
            var createdCircuit = await _circuitService.CreateCircuitAsync(newCircuit);

            // Create routes between consecutive sites
            for (int i = 0; i < optimalRoute.Count - 1; i++)
            {
                var currentTask = tasks[optimalRoute[i]];
                var nextTask = tasks[optimalRoute[i + 1]];

                var route = new RouteDto
                {
                    CircuitId = createdCircuit.CircuitId,
                    DepartureSiteId = currentTask.Site.SiteId,
                    ArrivalSiteId = nextTask.Site.SiteId,
                    DepartureSiteName = currentTask.Site.Name,
                    ArrivalSiteName = nextTask.Site.Name,
                    DistanceKm = matrixResponse.Distances[optimalRoute[i]][optimalRoute[i + 1]] / 1000.0,
                    Ordre = i + 1
                };

                await _routeService.CreateRouteAsync(route);
            }

            return true;
        }

        private List<int> FindOptimalRouteFromStart(List<List<double>> distances, int startIndex)
        {
            var n = distances.Count;
            var route = new List<int> { startIndex }; // Start with the specified first location
            var unvisited = Enumerable.Range(0, n).Where(x => x != startIndex).ToList();

            while (unvisited.Any())
            {
                var current = route.Last();
                var next = unvisited.OrderBy(x => distances[current][x]).First();
                route.Add(next);
                unvisited.Remove(next);
            }

            return route;
        }
    }
}
