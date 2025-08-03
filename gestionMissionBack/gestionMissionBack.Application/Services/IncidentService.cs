using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.DTOs.Incident;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Validators;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Application.DTOs.Notification;
using gestionMissionBack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Services
{
    public class IncidentService : IIncidentService
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IValidator<IncidentDto> _incidentValidator;
        private readonly IValidator<IncidentCreateDto> _incidentCreateValidator;
        private readonly IValidator<IncidentUpdateDto> _incidentUpdateValidator;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public IncidentService(
            IIncidentRepository incidentRepository,
            IValidator<IncidentDto> incidentValidator,
            IValidator<IncidentCreateDto> incidentCreateValidator,
            IValidator<IncidentUpdateDto> incidentUpdateValidator,
            IMapper mapper,
            INotificationService notificationService)
        {
            _incidentRepository = incidentRepository ?? throw new ArgumentNullException(nameof(incidentRepository));
            _incidentValidator = incidentValidator ?? throw new ArgumentNullException(nameof(incidentValidator));
            _incidentCreateValidator = incidentCreateValidator ?? throw new ArgumentNullException(nameof(incidentCreateValidator));
            _incidentUpdateValidator = incidentUpdateValidator ?? throw new ArgumentNullException(nameof(incidentUpdateValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public async Task<IncidentGetDto> GetIncidentByIdAsync(int id)
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null)
                throw new KeyNotFoundException($"Incident with ID {id} not found");

            return new IncidentGetDto
            {
                IncidentId = incident.IncidentId,
                MissionId = incident.MissionId,
                Type = incident.Type,
                Description = incident.Description,
                ReportDate = incident.ReportDate,
                Status = incident.Status,
                IncidentDocsUrls = !string.IsNullOrEmpty(incident.IncidentDocsUrls)
                    ? JsonSerializer.Deserialize<List<string>>(incident.IncidentDocsUrls) ?? new List<string>()
                    : new List<string>()
            };
        }

        public async Task<IEnumerable<IncidentGetDto>> GetAllIncidentsAsync()
        {
            var incidents = await _incidentRepository.GetAllAsync();
            return incidents.Select(incident => new IncidentGetDto
            {
                IncidentId = incident.IncidentId,
                MissionId = incident.MissionId,
                Type = incident.Type,
                Description = incident.Description,
                ReportDate = incident.ReportDate,
                Status = incident.Status,
                IncidentDocsUrls = !string.IsNullOrEmpty(incident.IncidentDocsUrls)
                    ? JsonSerializer.Deserialize<List<string>>(incident.IncidentDocsUrls) ?? new List<string>()
                    : new List<string>()
            });
        }

        public async Task<IEnumerable<IncidentGetDto>> GetIncidentsByMissionIdAsync(int missionId)
        {
            var incidents = await _incidentRepository.GetIncidentsByMissionIdAsync(missionId);
            return incidents.Select(incident => new IncidentGetDto
            {
                IncidentId = incident.IncidentId,
                MissionId = incident.MissionId,
                Type = incident.Type,
                Description = incident.Description,
                ReportDate = incident.ReportDate,
                Status = incident.Status,
                IncidentDocsUrls = !string.IsNullOrEmpty(incident.IncidentDocsUrls)
                    ? JsonSerializer.Deserialize<List<string>>(incident.IncidentDocsUrls) ?? new List<string>()
                    : new List<string>()
            });
        }

        public async Task<PagedResult<IncidentGetDto>> GetPagedAsync(int pageNumber, int pageSize, IncidentFilter filter = null)
        {
            var query = _incidentRepository.GetQueryable();

            if (filter != null)
            {
                if (filter.MissionId.HasValue)
                    query = query.Where(i => i.MissionId == filter.MissionId.Value);
                if (filter.Type.HasValue)
                    query = query.Where(i => i.Type == filter.Type.Value);
                if (filter.Status.HasValue)
                    query = query.Where(i => i.Status == filter.Status.Value);
                if (filter.ReportDateStart.HasValue)
                    query = query.Where(i => i.ReportDate >= filter.ReportDateStart.Value);
                if (filter.ReportDateEnd.HasValue)
                    query = query.Where(i => i.ReportDate <= filter.ReportDateEnd.Value);
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<IncidentGetDto>
            {
                Data = data.Select(incident => new IncidentGetDto
                {
                    IncidentId = incident.IncidentId,
                    MissionId = incident.MissionId,
                    Type = incident.Type,
                    Description = incident.Description,
                    ReportDate = incident.ReportDate,
                    Status = incident.Status,
                    IncidentDocsUrls = !string.IsNullOrEmpty(incident.IncidentDocsUrls)
                        ? JsonSerializer.Deserialize<List<string>>(incident.IncidentDocsUrls) ?? new List<string>()
                        : new List<string>()
                }),
                TotalRecords = totalRecords
            };
        }

        public async Task<IncidentGetDto> CreateIncidentAsync(IncidentCreateDto incidentDto)
        {
            var validationResult = await _incidentCreateValidator.ValidateAsync(incidentDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var incident = _mapper.Map<Incident>(incidentDto);
            
            // Handle document uploads
            if (incidentDto.IncidentDocs != null && incidentDto.IncidentDocs.Any())
            {
                var docUrls = new List<string>();
                foreach (var doc in incidentDto.IncidentDocs)
                {
                    if (doc != null && doc.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(doc.FileName);
                        var filePath = Path.Combine("wwwroot", "uploads", fileName);
                        
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await doc.CopyToAsync(stream);
                        }
                        
                        docUrls.Add($"/uploads/{fileName}");
                    }
                }
                incident.IncidentDocsUrls = JsonSerializer.Serialize(docUrls);
            }

            var createdIncident = await _incidentRepository.AddAsync(incident);
            
            // Get mission details for notification
            var mission = await _incidentRepository.GetQueryable()
                .Where(i => i.IncidentId == createdIncident.IncidentId)
                .Include(i => i.Mission)
                .ThenInclude(m => m.Requester)
                .Select(i => i.Mission)
                .FirstOrDefaultAsync();
            
            // Automatic notification when incident is created
            if (mission != null)
            {
                // Notify requester about the incident
                await _notificationService.SendRealTimeNotificationAsync(
                    mission.RequesterId,
                    new CreateNotificationDto
                    {
                        UserId = mission.RequesterId,
                        Title = "Incident Reported",
                        Message = $"An incident has been reported for mission #{mission.Service}: {createdIncident.Type}",
                        NotificationType = NotificationCategory.Incident,
                        Priority = NotificationPriority.High,
                        RelatedEntityType = "Incident",
                        RelatedEntityId = createdIncident.IncidentId
                    }
                );
            }
            
            return await GetIncidentByIdAsync(createdIncident.IncidentId);
        }

        public async Task UpdateIncidentAsync(int id, IncidentUpdateDto incidentDto)
        {
            var validationResult = await _incidentUpdateValidator.ValidateAsync(incidentDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingIncident = await _incidentRepository.GetQueryable()
                .Include(i => i.Mission)
                .ThenInclude(m => m.Requester)
                .FirstOrDefaultAsync(i => i.IncidentId == id);
                
            if (existingIncident == null)
                throw new KeyNotFoundException($"Incident with ID {id} not found");

            var oldStatus = existingIncident.Status;
            _mapper.Map(incidentDto, existingIncident);
            
            // Handle document uploads
            if (incidentDto.IncidentDocs != null && incidentDto.IncidentDocs.Any())
            {
                var docUrls = new List<string>();
                foreach (var doc in incidentDto.IncidentDocs)
                {
                    if (doc != null && doc.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(doc.FileName);
                        var filePath = Path.Combine("wwwroot", "uploads", fileName);
                        
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await doc.CopyToAsync(stream);
                        }
                        
                        docUrls.Add($"/uploads/{fileName}");
                    }
                }
                existingIncident.IncidentDocsUrls = JsonSerializer.Serialize(docUrls);
            }

            var updated = await _incidentRepository.UpdateAsync(existingIncident);
            
            // Automatic notification if status changed
            if (updated && oldStatus != existingIncident.Status)
            {
                // Notify requester about status change
                await _notificationService.SendRealTimeNotificationAsync(
                    existingIncident.Mission.RequesterId,
                    new CreateNotificationDto
                    {
                        UserId = existingIncident.Mission.RequesterId,
                        Title = "Incident Status Updated",
                        Message = $"Incident #{existingIncident.Description} status changed to {existingIncident.Status}",
                        NotificationType = NotificationCategory.Incident,
                        Priority = NotificationPriority.Normal,
                        RelatedEntityType = "Incident",
                        RelatedEntityId = id
                    }
                );
            }
            
            if (!updated)
            {
                throw new Exception($"Failed to update incident with ID {id}");
            }
        }

        public async Task DeleteIncidentAsync(int id)
        {
            var deleted = await _incidentRepository.DeleteAsync(id);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Incident with ID {id} not found or could not be deleted");
            }
        }

        public async Task<int> GetIncidentCountByMissionIdAsync(int missionId)
        {
            return await _incidentRepository.GetIncidentCountByMissionIdAsync(missionId);
        }
    }
}