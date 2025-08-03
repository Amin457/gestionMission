using gestionMissionBack.Application.DTOs.Route;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface IRouteService
    {
        Task<RouteDto> GetRouteByIdAsync(int id);
        Task<IEnumerable<RouteDto>> GetAllRoutesAsync();
        Task<IEnumerable<RouteDto>> GetRoutesByCircuitIdAsync(int circuitId);
        Task<double> GetTotalDistanceByCircuitIdAsync(int circuitId);
        Task<RouteDto> CreateRouteAsync(RouteDto routeDto);
        Task UpdateRouteAsync(RouteDto routeDto);
        Task DeleteRouteAsync(int id);
        Task DeleteRoutesByCircuitIdAsync(int circuitId);
    }
}