using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestionMissionBack.Infrastructure.Repositories
{
    public class SiteRepository : GenericRepository<Site>, ISiteRepository
    {
        private readonly MissionFleetContext _context;

        public SiteRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Site>> GetSitesByCityIdAsync(int cityId)
        {
            return await _context.Sites
                .Where(s => s.CityId == cityId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Site>> GetSitesByTypeAsync(string type)
        {
            return await _context.Sites
                .Where(s => s.Type.ToLower() == type.ToLower())
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
