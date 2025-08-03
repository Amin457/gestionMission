using gestionMissionBack.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface IVehicleReservationRepository : IGenericRepository<VehicleReservation>
    {
        IQueryable<VehicleReservation> GetQueryable();
        Task<IEnumerable<VehicleReservation>> GetReservationsByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<VehicleReservation>> GetReservationsByRequesterIdAsync(int requesterId);
        Task<IEnumerable<VehicleReservation>> GetReservationsByStatusAsync(string status);
    }
}