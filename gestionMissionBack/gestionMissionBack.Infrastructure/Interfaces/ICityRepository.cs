using gestionMissionBack.Domain.Entities;

namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface ICityRepository : IGenericRepository<City>
    {
        Task<City> GetCityWithSitesAsync(int cityId);
        Task<IEnumerable<City>> GetCitiesWithRegionAsync(string region);
        Task<City> FindByNameAsync(string Name);

    }
}
