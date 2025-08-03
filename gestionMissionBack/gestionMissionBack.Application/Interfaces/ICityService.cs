using gestionMissionBack.Application.DTOs.City;
using gestionMissionBack.Domain.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface ICityService
    {
        Task<CityDto> GetCityByIdAsync(int id);
        Task<IEnumerable<CityDto>> GetAllCitiesAsync();
        Task<CityDto> GetCityWithSitesAsync(int cityId);
        Task<IEnumerable<CityDto>> GetCitiesWithRegionAsync(string region);
        Task<CityDto> CreateCityAsync(CityDto cityDto);
        Task UpdateCityAsync(CityDto cityDto);
        Task DeleteCityAsync(int id);
        Task<PagedResult<CityDto>> GetPagedAsync(int pageNumber, int pageSize);
    }
}
