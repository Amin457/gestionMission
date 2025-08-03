using gestionMissionBack.Application.DTOs.VehicleReservation;
using gestionMissionBack.Domain.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface IVehicleReservationService
    {
        Task<VehicleReservationDtoGet> GetReservationByIdAsync(int id);
        Task<IEnumerable<VehicleReservationDtoGet>> GetAllReservationsAsync();
        Task<PagedResult<VehicleReservationDtoGet>> GetPagedAsync(int pageNumber, int pageSize, VehicleReservationFilter filter);
        Task<IEnumerable<VehicleReservationDtoGet>> GetReservationsByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<VehicleReservationDtoGet>> GetReservationsByRequesterIdAsync(int requesterId);
        Task<IEnumerable<VehicleReservationDtoGet>> GetReservationsByStatusAsync(string status);
        Task<VehicleReservationDto> CreateReservationAsync(VehicleReservationDto reservationDto);
        Task UpdateReservationAsync(VehicleReservationDto reservationDto);
        Task<bool> ApproveReservationAsync(int reservationId);
        Task DeleteReservationAsync(int id);
    }
}