using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestionMissionBack.Infrastructure.Repositories
{
    public class CityRepository : GenericRepository<City>, ICityRepository
    {
        private readonly MissionFleetContext _context;

        public CityRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        public async Task<City> GetCityWithSitesAsync(int cityId)
        {
            return await _context.Cities
                .Include(c => c.Sites)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CityId == cityId);
        }

        public async Task<IEnumerable<City>> GetCitiesWithRegionAsync(string region)
        {
            return await _context.Cities
                .Where(c => c.Region != null && c.Region.ToLower() == region.ToLower())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<City> FindByNameAsync(string roleName)
        {
            return await _context.Cities
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
        }
    }
}
