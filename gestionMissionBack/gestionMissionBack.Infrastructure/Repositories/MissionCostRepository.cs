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
    public class MissionCostRepository : GenericRepository<MissionCost>, IMissionCostRepository
    {
        private readonly MissionFleetContext _context;

        public MissionCostRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MissionCost>> GetCostsByMissionIdAsync(int missionId)
        {
            return await _context.MissionCosts
                .AsNoTracking()
                .Where(mc => mc.MissionId == missionId)
                .ToListAsync();
        }

        public async Task<double> GetTotalCostByMissionIdAsync(int missionId)
        {
            return await _context.MissionCosts
                .AsNoTracking()
                .Where(mc => mc.MissionId == missionId)
                .SumAsync(mc => mc.Amount);
        }

        public IQueryable<MissionCost> GetQueryable()
        {
            return _context.MissionCosts.AsQueryable();
        }
    }
}