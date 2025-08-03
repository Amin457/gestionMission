using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.DTOs.Vehicle;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Validators;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Domain.DTOs;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IValidator<VehicleCreateDto> _vehicleCreateValidator;
        private readonly IValidator<VehicleUpdateDto> _vehicleUpdateValidator;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public VehicleService(
            IVehicleRepository vehicleRepository,
            IValidator<VehicleCreateDto> vehicleCreateValidator,
            IValidator<VehicleUpdateDto> vehicleUpdateValidator,
            IMapper mapper,
            IWebHostEnvironment env)
        {
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            _vehicleCreateValidator = vehicleCreateValidator ?? throw new ArgumentNullException(nameof(vehicleCreateValidator));
            _vehicleUpdateValidator = vehicleUpdateValidator ?? throw new ArgumentNullException(nameof(vehicleUpdateValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task<VehicleGetDto> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
                throw new KeyNotFoundException($"Vehicle with ID {id} not found");

            return new VehicleGetDto
            {
                VehicleId = vehicle.VehicleId,
                Type = vehicle.Type,
                LicensePlate = vehicle.LicensePlate,
                Availability = vehicle.Availability,
                MaxCapacity = vehicle.MaxCapacity,
                PhotoUrls = !string.IsNullOrEmpty(vehicle.PhotoUrls) 
                    ? JsonSerializer.Deserialize<List<string>>(vehicle.PhotoUrls) ?? new List<string>()
                    : new List<string>()
            };
        }

        public async Task<IEnumerable<VehicleGetDto>> GetAllVehiclesAsync()
        {
            var vehicles = await _vehicleRepository.GetAllAsync();
            return vehicles.Select(vehicle => new VehicleGetDto
            {
                VehicleId = vehicle.VehicleId,
                Type = vehicle.Type,
                LicensePlate = vehicle.LicensePlate,
                Availability = vehicle.Availability,
                MaxCapacity = vehicle.MaxCapacity,
                PhotoUrls = !string.IsNullOrEmpty(vehicle.PhotoUrls) 
                    ? JsonSerializer.Deserialize<List<string>>(vehicle.PhotoUrls) ?? new List<string>()
                    : new List<string>()
            });
        }

        public async Task<IEnumerable<VehicleGetDto>> GetAvailableVehiclesAsync()
        {
            var vehicles = await _vehicleRepository.GetAvailableVehiclesAsync();
            return vehicles.Select(vehicle => new VehicleGetDto
            {
                VehicleId = vehicle.VehicleId,
                Type = vehicle.Type,
                LicensePlate = vehicle.LicensePlate,
                Availability = vehicle.Availability,
                MaxCapacity = vehicle.MaxCapacity,
                PhotoUrls = !string.IsNullOrEmpty(vehicle.PhotoUrls) 
                    ? JsonSerializer.Deserialize<List<string>>(vehicle.PhotoUrls) ?? new List<string>()
                    : new List<string>()
            });
        }

        public async Task<int> GetVehicleCountByTypeAsync(VehicleType type)
        {
            return await _vehicleRepository.GetVehicleCountByTypeAsync(type);
        }

        public async Task<VehicleGetDto> CreateVehicleAsync(VehicleCreateDto vehicleCreateDto)
        {
            var validationResult = await _vehicleCreateValidator.ValidateAsync(vehicleCreateDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var photoUrls = new List<string>();
            if (vehicleCreateDto.Photos != null && vehicleCreateDto.Photos.Any())
            {
                foreach (var photo in vehicleCreateDto.Photos)
                {
                    if (photo != null && photo.Length > 0)
                    {
                        var fileName = await StoreFileAsync(photo);
                        photoUrls.Add($"uploads/{fileName}");
                    }
                }
            }

            var vehicle = new Vehicle
            {
                Type = vehicleCreateDto.Type,
                LicensePlate = vehicleCreateDto.LicensePlate,
                Availability = vehicleCreateDto.Availability,
                MaxCapacity = vehicleCreateDto.MaxCapacity,
                PhotoUrls = JsonSerializer.Serialize(photoUrls)
            };

            var createdVehicle = await _vehicleRepository.AddAsync(vehicle);
            return await GetVehicleByIdAsync(createdVehicle.VehicleId);
        }

        public async Task UpdateVehicleAsync(VehicleUpdateDto vehicleUpdateDto)
        {
            var validationResult = await _vehicleUpdateValidator.ValidateAsync(vehicleUpdateDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingVehicle = await _vehicleRepository.GetByIdAsync(vehicleUpdateDto.VehicleId);
            if (existingVehicle == null)
                throw new KeyNotFoundException($"Vehicle with ID {vehicleUpdateDto.VehicleId} not found");

            // Keep existing photos
            var photoUrls = vehicleUpdateDto.KeepPhotosUrls ?? new List<string>();

            // Add new photos
            if (vehicleUpdateDto.NewPhotos != null && vehicleUpdateDto.NewPhotos.Any())
            {
                foreach (var photo in vehicleUpdateDto.NewPhotos)
                {
                    if (photo != null && photo.Length > 0)
                    {
                        var fileName = await StoreFileAsync(photo);
                        photoUrls.Add($"uploads/{fileName}");
                    }
                }
            }

            existingVehicle.Type = vehicleUpdateDto.Type;
            existingVehicle.LicensePlate = vehicleUpdateDto.LicensePlate;
            existingVehicle.Availability = vehicleUpdateDto.Availability;
            existingVehicle.MaxCapacity = vehicleUpdateDto.MaxCapacity;
            existingVehicle.PhotoUrls = JsonSerializer.Serialize(photoUrls);

            var updated = await _vehicleRepository.UpdateAsync(existingVehicle);
            if (!updated)
            {
                throw new Exception($"Failed to update vehicle with ID {vehicleUpdateDto.VehicleId}");
            }
        }

        public async Task DeleteVehicleAsync(int id)
        {
            var deleted = await _vehicleRepository.DeleteAsync(id);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Vehicle with ID {id} not found or could not be deleted");
            }
        }

        public async Task<PagedResult<VehicleGetDto>> GetPagedAsync(int pageNumber, int pageSize, VehicleFilter filter = null)
        {
            var vehicles = await _vehicleRepository.GetPagedAsync(pageNumber, pageSize, filter);

            return new PagedResult<VehicleGetDto>
            {
                Data = vehicles.Data.Select(vehicle => new VehicleGetDto
                {
                    VehicleId = vehicle.VehicleId,
                    Type = vehicle.Type,
                    LicensePlate = vehicle.LicensePlate,
                    Availability = vehicle.Availability,
                    MaxCapacity = vehicle.MaxCapacity,
                    PhotoUrls = !string.IsNullOrEmpty(vehicle.PhotoUrls) 
                        ? JsonSerializer.Deserialize<List<string>>(vehicle.PhotoUrls) ?? new List<string>()
                        : new List<string>()
                }),
                TotalRecords = vehicles.TotalRecords
            };
        }

        private async Task<string> StoreFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}