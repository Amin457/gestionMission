using gestionMissionBack.Domain.Entities;

namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface IRouteRepository : IGenericRepository<Route>
    {
        Task<IEnumerable<Route>> GetRoutesByCircuitIdAsync(int circuitId);
        Task<double> GetTotalDistanceByCircuitIdAsync(int circuitId);
    }
}