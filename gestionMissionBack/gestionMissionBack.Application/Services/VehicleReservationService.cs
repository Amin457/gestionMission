using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.DTOs.VehicleReservation;
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
using System.Threading.Tasks;
using gestionMissionBack.Application.DTOs.Vehicle;

namespace gestionMissionBack.Application.Services
{
    public class VehicleReservationService : IVehicleReservationService
    {
        private readonly IVehicleReservationRepository _reservationRepository;
        private readonly IValidator<VehicleReservationDto> _reservationValidator;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleReservationService(
            IVehicleReservationRepository reservationRepository,
            IValidator<VehicleReservationDto> reservationValidator,
            IMapper mapper,
            INotificationService notificationService,
            IVehicleRepository vehicleRepository)
        {
            _reservationRepository = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
            _reservationValidator = reservationValidator ?? throw new ArgumentNullException(nameof(reservationValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _vehicleRepository = vehicleRepository;
        }

        public async Task<VehicleReservationDtoGet> GetReservationByIdAsync(int id)
        {
            var query = _reservationRepository.GetQueryable();
            var reservation = await query
                .Include(r => r.Requester)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.ReservationId == id);
            return _mapper.Map<VehicleReservationDtoGet>(reservation);
        }

        public async Task<PagedResult<VehicleReservationDtoGet>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await GetPagedAsync(pageNumber, pageSize, null);
        }

        public async Task<PagedResult<VehicleReservationDtoGet>> GetPagedAsync(int pageNumber, int pageSize, VehicleReservationFilter filter)
        {
            var query = _reservationRepository.GetQueryable();

            // Apply filters
            if (filter != null)
            {
                if (filter.RequesterId.HasValue)
                    query = query.Where(r => r.RequesterId == filter.RequesterId.Value);

                if (filter.VehicleId.HasValue)
                    query = query.Where(r => r.VehicleId == filter.VehicleId.Value);

                if (filter.Status.HasValue)
                    query = query.Where(r => r.Status == filter.Status.Value);

                if (filter.RequiresDriver.HasValue)
                    query = query.Where(r => r.RequiresDriver == filter.RequiresDriver.Value);

                if (filter.StartDateFrom.HasValue)
                    query = query.Where(r => r.StartDate >= filter.StartDateFrom.Value);

                if (filter.StartDateTo.HasValue)
                    query = query.Where(r => r.StartDate <= filter.StartDateTo.Value);

                if (filter.EndDateFrom.HasValue)
                    query = query.Where(r => r.EndDate >= filter.EndDateFrom.Value);

                if (filter.EndDateTo.HasValue)
                    query = query.Where(r => r.EndDate <= filter.EndDateTo.Value);

                if (!string.IsNullOrEmpty(filter.Departure))
                    query = query.Where(r => r.Departure.Contains(filter.Departure));

                if (!string.IsNullOrEmpty(filter.Destination))
                    query = query.Where(r => r.Destination.Contains(filter.Destination));
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .Include(r => r.Requester)
                .Include(r => r.Vehicle)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<VehicleReservationDtoGet>
            {
                Data = _mapper.Map<IEnumerable<VehicleReservationDtoGet>>(data),
                TotalRecords = totalRecords
            };
        }

        public async Task<IEnumerable<VehicleReservationDtoGet>> GetAllReservationsAsync()
        {
            var query = _reservationRepository.GetQueryable();
            var reservations = await query
                .Include(r => r.Requester)
                .Include(r => r.Vehicle)
                .ToListAsync();
            return _mapper.Map<IEnumerable<VehicleReservationDtoGet>>(reservations);
        }

        public async Task<IEnumerable<VehicleReservationDtoGet>> GetReservationsByVehicleIdAsync(int vehicleId)
        {
            var query = _reservationRepository.GetQueryable();
            var reservations = await query
                .Include(r => r.Requester)
                .Include(r => r.Vehicle)
                .Where(r => r.VehicleId == vehicleId)
                .ToListAsync();
            return _mapper.Map<IEnumerable<VehicleReservationDtoGet>>(reservations);
        }

        public async Task<IEnumerable<VehicleReservationDtoGet>> GetReservationsByRequesterIdAsync(int requesterId)
        {
            var query = _reservationRepository.GetQueryable();
            var reservations = await query
                .Include(r => r.Requester)
                .Include(r => r.Vehicle)
                .Where(r => r.RequesterId == requesterId)
                .ToListAsync();
            return _mapper.Map<IEnumerable<VehicleReservationDtoGet>>(reservations);
        }

        public async Task<IEnumerable<VehicleReservationDtoGet>> GetReservationsByStatusAsync(string status)
        {
            var query = _reservationRepository.GetQueryable();
            var reservations = await query
                .Include(r => r.Requester)
                .Include(r => r.Vehicle)
                .Where(r => r.Status.ToString() == status)
                .ToListAsync();
            return _mapper.Map<IEnumerable<VehicleReservationDtoGet>>(reservations);
        }

        public async Task<VehicleReservationDto> CreateReservationAsync(VehicleReservationDto reservationDto)
        {
            var validationResult = await _reservationValidator.ValidateAsync(reservationDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var reservation = _mapper.Map<VehicleReservation>(reservationDto);
            var createdReservation = await _reservationRepository.AddAsync(reservation);
            
            // Automatic notification to requester when reservation is created
            // should notify Admin
            //await _notificationService.SendRealTimeNotificationAsync(
            //    createdReservation.RequesterId,
            //    new CreateNotificationDto
            //    {
            //        UserId = createdReservation.RequesterId,
            //        Title = "Vehicle Reservation Created",
            //        Message = $"Your vehicle reservation has been created successfully",
            //        NotificationType = NotificationCategory.Reservation,
            //        Priority = NotificationPriority.Normal,
            //        RelatedEntityType = "VehicleReservation",
            //        RelatedEntityId = createdReservation.ReservationId
            //    }
            //);
            
            return _mapper.Map<VehicleReservationDto>(createdReservation);
        }

        public async Task UpdateReservationAsync(VehicleReservationDto reservationDto)
        {
            var validationResult = await _reservationValidator.ValidateAsync(reservationDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingReservation = await _reservationRepository.GetByIdAsync(reservationDto.ReservationId);
            var oldStatus = existingReservation.Status;
            
            _mapper.Map(reservationDto, existingReservation);
            var updated = await _reservationRepository.UpdateAsync(existingReservation);
            
            // Automatic notification if status changed
            if (updated && oldStatus != existingReservation.Status)
            {
                await _notificationService.SendRealTimeNotificationAsync(
                    existingReservation.RequesterId,
                    new CreateNotificationDto
                    {
                        UserId = existingReservation.RequesterId,
                        Title = "Vehicle Reservation Status Updated",
                        Message = $"Your vehicle reservation status changed to {existingReservation.Status}",
                        NotificationType = NotificationCategory.Reservation,
                        Priority = NotificationPriority.Normal,
                        RelatedEntityType = "VehicleReservation",
                        RelatedEntityId = existingReservation.ReservationId
                    }
                );
            }
            
            if (!updated)
            {
                throw new Exception($"Failed to update reservation with ID {reservationDto.ReservationId}");
            }
        }

        public async Task<bool> ApproveReservationAsync(int reservationId)
        {
            var reservation = await _reservationRepository.GetQueryable()
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
            
            if (reservation == null)
                return false;

            reservation.Status = VehicleReservationStatus.Approved;
            await _reservationRepository.UpdateAsync(reservation);
            
            var existingVehicle = await _vehicleRepository.GetByIdAsync(reservation.VehicleId);
            existingVehicle.Availability =false;
            await _vehicleRepository.UpdateAsync(existingVehicle);

            // Automatic notification to requester when reservation is approved
            await _notificationService.SendRealTimeNotificationAsync(
                reservation.RequesterId,
                new CreateNotificationDto
                {
                    UserId = reservation.RequesterId,
                    Title = "Vehicle Reservation Approved",
                    Message = $"Your vehicle reservation for {reservation.Vehicle.LicensePlate} has been approved",
                    NotificationType = NotificationCategory.Reservation,
                    Priority = NotificationPriority.Normal,
                    RelatedEntityType = "VehicleReservation",
                    RelatedEntityId = reservationId
                }
            );
            
            return true;
        }

        public async Task DeleteReservationAsync(int id)
        {
            var deleted = await _reservationRepository.DeleteAsync(id);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Reservation with ID {id} not found or could not be deleted");
            }
        }
    }
}