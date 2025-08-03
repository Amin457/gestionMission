using gestionMissionBack.Application.DTOs;
using gestionMissionBack.Application.DTOs.Site;
using gestionMissionBack.Domain.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface ISiteService
    {
        Task<SiteDtoGet> GetSiteByIdAsync(int id);
        Task<IEnumerable<SiteDtoGet>> GetAllSitesAsync();
        Task<IEnumerable<SiteDtoGet>> GetSitesByCityIdAsync(int cityId);
        Task<IEnumerable<SiteDtoGet>> GetSitesByTypeAsync(string type);
        Task<SiteDto> CreateSiteAsync(SiteDto siteDto);
        Task UpdateSiteAsync(SiteDto siteDto);
        Task DeleteSiteAsync(int id);
        Task<PagedResult<SiteDtoGet>> GetPagedAsync(int pageNumber, int pageSize);
    }
}
