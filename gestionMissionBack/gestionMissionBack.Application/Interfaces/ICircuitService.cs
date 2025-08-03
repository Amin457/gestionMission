using gestionMissionBack.Application.DTOs.Circuit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface ICircuitService
    {
        Task<CircuitDto> GetCircuitByIdAsync(int id);
        Task<IEnumerable<CircuitDto>> GetAllCircuitsAsync();
        Task<IEnumerable<CircuitDto>> GetCircuitsByMissionIdAsync(int missionId);
        Task<CircuitDto> GetCircuitByMissionIdAsync(int missionId);
        Task<CircuitDto> CreateCircuitAsync(CircuitDto circuitDto);
        Task UpdateCircuitAsync(CircuitDto circuitDto);
        Task DeleteCircuitAsync(int id);
        Task<int> GetCircuitCountByMissionIdAsync(int missionId);
    }
}