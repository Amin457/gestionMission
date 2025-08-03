using gestionMissionBack.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface IMissionCostRepository : IGenericRepository<MissionCost>
    {
        Task<IEnumerable<MissionCost>> GetCostsByMissionIdAsync(int missionId);
        Task<double> GetTotalCostByMissionIdAsync(int missionId);
        IQueryable<MissionCost> GetQueryable();
    }
}