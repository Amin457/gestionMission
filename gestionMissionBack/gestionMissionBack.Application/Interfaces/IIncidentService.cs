using gestionMissionBack.Application.DTOs.Incident;
using gestionMissionBack.Domain.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface IIncidentService
    {
        Task<IncidentGetDto> GetIncidentByIdAsync(int id);
        Task<IEnumerable<IncidentGetDto>> GetAllIncidentsAsync();
        Task<IEnumerable<IncidentGetDto>> GetIncidentsByMissionIdAsync(int missionId);
        Task<PagedResult<IncidentGetDto>> GetPagedAsync(int pageNumber, int pageSize, IncidentFilter filter = null);
        Task<IncidentGetDto> CreateIncidentAsync(IncidentCreateDto incidentDto);
        Task UpdateIncidentAsync(int id, IncidentUpdateDto incidentDto);
        Task DeleteIncidentAsync(int id);
        Task<int> GetIncidentCountByMissionIdAsync(int missionId);
    }
}