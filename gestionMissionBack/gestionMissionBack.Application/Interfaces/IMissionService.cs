using gestionMissionBack.Application.DTOs.Mission;
using gestionMissionBack.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface IMissionService
    {
        Task<MissionDto> GetMissionByIdAsync(int id);
        Task<MissionDto> CreateMissionAsync(MissionDto missionDto);
        Task<bool> UpdateMissionAsync(MissionDto missionDto);
        Task<bool> DeleteMissionAsync(int id);
        Task<PagedResult<MissionDtoGet>> GetPagedAsync(int pageNumber, int pageSize, MissionFilter filters = null);
        Task<bool> GenerateCircuitsForMissionAsync(int missionId);
    }
}
