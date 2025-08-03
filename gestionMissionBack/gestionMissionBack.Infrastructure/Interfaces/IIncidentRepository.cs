using gestionMissionBack.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface IIncidentRepository : IGenericRepository<Incident>
    {
        Task<IEnumerable<Incident>> GetIncidentsByMissionIdAsync(int missionId);
        Task<int> GetIncidentCountByMissionIdAsync(int missionId);
        IQueryable<Incident> GetQueryable();
    }
}