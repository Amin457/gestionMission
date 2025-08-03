using gestionMissionBack.Application.DTOs.Vehicle;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<VehicleGetDto> GetVehicleByIdAsync(int id);
        Task<IEnumerable<VehicleGetDto>> GetAllVehiclesAsync();
        Task<IEnumerable<VehicleGetDto>> GetAvailableVehiclesAsync();
        Task<int> GetVehicleCountByTypeAsync(VehicleType type);
        Task<VehicleGetDto> CreateVehicleAsync(VehicleCreateDto vehicleCreateDto);
        Task UpdateVehicleAsync(VehicleUpdateDto vehicleUpdateDto);
        Task DeleteVehicleAsync(int id);
        Task<PagedResult<VehicleGetDto>> GetPagedAsync(int pageNumber, int pageSize, VehicleFilter filter = null);
    }
}