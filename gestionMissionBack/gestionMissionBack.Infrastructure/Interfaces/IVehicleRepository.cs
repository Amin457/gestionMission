using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Domain.DTOs;

namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync();
        Task<int> GetVehicleCountByTypeAsync(VehicleType type);
        Task<PagedResult<Vehicle>> GetPagedAsync(int pageNumber, int pageSize, VehicleFilter filter = null);
    }
}