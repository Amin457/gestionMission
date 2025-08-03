using gestionMissionBack.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface ICircuitRepository : IGenericRepository<Circuit>
    {
        Task<IEnumerable<Circuit>> GetCircuitsByMissionIdAsync(int missionId);
        Task<Circuit> GetCircuitByMissionIdAsync(int missionId);
        Task<int> GetCircuitCountByMissionIdAsync(int missionId);
    }
}