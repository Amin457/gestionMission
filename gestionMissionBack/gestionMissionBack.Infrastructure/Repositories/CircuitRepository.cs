using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestionMissionBack.Infrastructure.Repositories
{
    public class CircuitRepository : GenericRepository<Circuit>, ICircuitRepository
    {
        private readonly MissionFleetContext _context;

        public CircuitRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Circuit>> GetCircuitsByMissionIdAsync(int missionId)
        {
            return await _context.Circuits
                .Where(c => c.MissionId == missionId)
                .Include(c => c.DepartureSite)
                .Include(c => c.ArrivalSite)
                .Include(c => c.Routes)
                    .ThenInclude(r => r.DepartureSite)
                .Include(c => c.Routes)
                    .ThenInclude(r => r.ArrivalSite)
                .ToListAsync();
        }

        public async Task<Circuit> GetCircuitByMissionIdAsync(int missionId)
        {
            return await _context.Circuits
                .Where(c => c.MissionId == missionId)
                .Include(c => c.DepartureSite)
                .Include(c => c.ArrivalSite)
                .Include(c => c.Routes)
                    .ThenInclude(r => r.DepartureSite)
                .Include(c => c.Routes)
                    .ThenInclude(r => r.ArrivalSite)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetCircuitCountByMissionIdAsync(int missionId)
        {
            return await _context.Circuits
                .CountAsync(c => c.MissionId == missionId);
        }
    }
}