using gestionMissionBack.Application.DTOs.MissionCost;
using gestionMissionBack.Domain.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface IMissionCostService
    {
        Task<MissionCostGetDto> GetCostByIdAsync(int id);
        Task<IEnumerable<MissionCostGetDto>> GetAllCostsAsync();
        Task<IEnumerable<MissionCostGetDto>> GetCostsByMissionIdAsync(int missionId);
        Task<PagedResult<MissionCostGetDto>> GetPagedAsync(int pageNumber, int pageSize, MissionCostFilter filter = null);
        Task<MissionCostGetDto> CreateCostAsync(MissionCostCreateDto costDto);
        Task UpdateCostAsync(int id, MissionCostUpdateDto costDto);
        Task DeleteCostAsync(int id);
        Task<double> GetTotalCostByMissionIdAsync(int missionId);
    }
}