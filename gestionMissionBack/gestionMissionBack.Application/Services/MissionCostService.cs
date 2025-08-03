using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.DTOs.MissionCost;
using gestionMissionBack.Application.DTOs.Notification;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Validators;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Services
{
    public class MissionCostService : IMissionCostService
    {
        private readonly IMissionCostRepository _missionCostRepository;
        private readonly IValidator<MissionCostDto> _missionCostValidator;
        private readonly IValidator<MissionCostCreateDto> _missionCostCreateValidator;
        private readonly IValidator<MissionCostUpdateDto> _missionCostUpdateValidator;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;


        public MissionCostService(
            IMissionCostRepository missionCostRepository,
            IValidator<MissionCostDto> missionCostValidator,
            IValidator<MissionCostCreateDto> missionCostCreateValidator,
            IValidator<MissionCostUpdateDto> missionCostUpdateValidator,
            IMapper mapper,
            INotificationService notificationService)
        {
            _missionCostRepository = missionCostRepository ?? throw new ArgumentNullException(nameof(missionCostRepository));
            _missionCostValidator = missionCostValidator ?? throw new ArgumentNullException(nameof(missionCostValidator));
            _missionCostCreateValidator = missionCostCreateValidator ?? throw new ArgumentNullException(nameof(missionCostCreateValidator));
            _missionCostUpdateValidator = missionCostUpdateValidator ?? throw new ArgumentNullException(nameof(missionCostUpdateValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _notificationService = notificationService;
        }

        public async Task<MissionCostGetDto> GetCostByIdAsync(int id)
        {
            var cost = await _missionCostRepository.GetByIdAsync(id);
            if (cost == null)
                throw new KeyNotFoundException($"Cost with ID {id} not found");

            return new MissionCostGetDto
            {
                CostId = cost.CostId,
                MissionId = cost.MissionId,
                Type = cost.Type,
                Amount = cost.Amount,
                Date = cost.Date,
                ReceiptPhotoUrls = !string.IsNullOrEmpty(cost.ReceiptPhotoUrls)
                    ? JsonSerializer.Deserialize<List<string>>(cost.ReceiptPhotoUrls) ?? new List<string>()
                    : new List<string>()
            };
        }

        public async Task<IEnumerable<MissionCostGetDto>> GetAllCostsAsync()
        {
            var costs = await _missionCostRepository.GetAllAsync();
            return costs.Select(cost => new MissionCostGetDto
            {
                CostId = cost.CostId,
                MissionId = cost.MissionId,
                Type = cost.Type,
                Amount = cost.Amount,
                Date = cost.Date,
                ReceiptPhotoUrls = !string.IsNullOrEmpty(cost.ReceiptPhotoUrls)
                    ? JsonSerializer.Deserialize<List<string>>(cost.ReceiptPhotoUrls) ?? new List<string>()
                    : new List<string>()
            });
        }

        public async Task<IEnumerable<MissionCostGetDto>> GetCostsByMissionIdAsync(int missionId)
        {
            var costs = await _missionCostRepository.GetCostsByMissionIdAsync(missionId);
            return costs.Select(cost => new MissionCostGetDto
            {
                CostId = cost.CostId,
                MissionId = cost.MissionId,
                Type = cost.Type,
                Amount = cost.Amount,
                Date = cost.Date,
                ReceiptPhotoUrls = !string.IsNullOrEmpty(cost.ReceiptPhotoUrls)
                    ? JsonSerializer.Deserialize<List<string>>(cost.ReceiptPhotoUrls) ?? new List<string>()
                    : new List<string>()
            });
        }

        public async Task<PagedResult<MissionCostGetDto>> GetPagedAsync(int pageNumber, int pageSize, MissionCostFilter filter = null)
        {
            var query = _missionCostRepository.GetQueryable();

            if (filter != null)
            {
                if (filter.MissionId.HasValue)
                    query = query.Where(mc => mc.MissionId == filter.MissionId.Value);
                if (filter.Type.HasValue)
                    query = query.Where(mc => mc.Type == filter.Type.Value);
                if (filter.MinAmount.HasValue)
                    query = query.Where(mc => mc.Amount >= filter.MinAmount.Value);
                if (filter.MaxAmount.HasValue)
                    query = query.Where(mc => mc.Amount <= filter.MaxAmount.Value);
                if (filter.DateStart.HasValue)
                    query = query.Where(mc => mc.Date >= filter.DateStart.Value);
                if (filter.DateEnd.HasValue)
                    query = query.Where(mc => mc.Date <= filter.DateEnd.Value);
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<MissionCostGetDto>
            {
                Data = data.Select(cost => new MissionCostGetDto
                {
                    CostId = cost.CostId,
                    MissionId = cost.MissionId,
                    Type = cost.Type,
                    Amount = cost.Amount,
                    Date = cost.Date,
                    ReceiptPhotoUrls = !string.IsNullOrEmpty(cost.ReceiptPhotoUrls)
                        ? JsonSerializer.Deserialize<List<string>>(cost.ReceiptPhotoUrls) ?? new List<string>()
                        : new List<string>()
                }),
                TotalRecords = totalRecords
            };
        }

        public async Task<MissionCostGetDto> CreateCostAsync(MissionCostCreateDto costDto)
        {
            var validationResult = await _missionCostCreateValidator.ValidateAsync(costDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var cost = _mapper.Map<MissionCost>(costDto);

            // Handle photo uploads
            if (costDto.ReceiptPhotos != null && costDto.ReceiptPhotos.Any())
            {
                var photoUrls = new List<string>();
                foreach (var photo in costDto.ReceiptPhotos)
                {
                    if (photo != null && photo.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                        var filePath = Path.Combine("wwwroot", "uploads", fileName);

                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }

                        photoUrls.Add($"/uploads/{fileName}");
                    }
                }
                cost.ReceiptPhotoUrls = JsonSerializer.Serialize(photoUrls);
            }

            var createdCost = await _missionCostRepository.AddAsync(cost);

            // Get mission details for notification
            var mission = await _missionCostRepository.GetQueryable()
                .Where(i => i.CostId == createdCost.CostId)
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
                        Title = "New Costs added !",
                        Message = $"Cost has been created for mission #{mission.Service}: {createdCost.Type}",
                        NotificationType = NotificationCategory.Alert,
                        Priority = NotificationPriority.Normal,
                        RelatedEntityType = "MissionCosts",
                        RelatedEntityId = createdCost.CostId
                    });

            }
            return await GetCostByIdAsync(createdCost.CostId);

        }
        public async Task<MissionCostGetDto> CreateCostJsonAsync(MissionCostDto costDto)
        {
            var validationResult = await _missionCostValidator.ValidateAsync(costDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var cost = _mapper.Map<MissionCost>(costDto);
            var createdCost = await _missionCostRepository.AddAsync(cost);
            return await GetCostByIdAsync(createdCost.CostId);
        }

        public async Task UpdateCostAsync(int id, MissionCostUpdateDto costDto)
        {
            var validationResult = await _missionCostUpdateValidator.ValidateAsync(costDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingCost = await _missionCostRepository.GetByIdAsync(id);
            if (existingCost == null)
                throw new KeyNotFoundException($"Cost with ID {id} not found");

            _mapper.Map(costDto, existingCost);
            
            // Handle photo uploads
            if (costDto.ReceiptPhotos != null && costDto.ReceiptPhotos.Any())
            {
                var photoUrls = new List<string>();
                foreach (var photo in costDto.ReceiptPhotos)
                {
                    if (photo != null && photo.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                        var filePath = Path.Combine("wwwroot", "uploads", fileName);
                        
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }
                        
                        photoUrls.Add($"/uploads/{fileName}");
                    }
                }
                existingCost.ReceiptPhotoUrls = JsonSerializer.Serialize(photoUrls);
            }

            var updated = await _missionCostRepository.UpdateAsync(existingCost);
            if (!updated)
            {
                throw new Exception($"Failed to update cost with ID {id}");
            }
        }

        public async Task DeleteCostAsync(int id)
        {
            var deleted = await _missionCostRepository.DeleteAsync(id);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Cost with ID {id} not found or could not be deleted");
            }
        }

        public async Task<double> GetTotalCostByMissionIdAsync(int missionId)
        {
            return await _missionCostRepository.GetTotalCostByMissionIdAsync(missionId);
        }
    }
}