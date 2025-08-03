using gestionMissionBack.Domain.Entities;

namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface ISiteRepository : IGenericRepository<Site>
    {
        Task<IEnumerable<Site>> GetSitesByCityIdAsync(int cityId);
        Task<IEnumerable<Site>> GetSitesByTypeAsync(string type);
    }
}
