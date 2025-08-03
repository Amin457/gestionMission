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
    public class IncidentRepository : GenericRepository<Incident>, IIncidentRepository
    {
        private readonly MissionFleetContext _context;

        public IncidentRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Incident>> GetIncidentsByMissionIdAsync(int missionId)
        {
            return await _context.Incidents
                .AsNoTracking()
                .Where(i => i.MissionId == missionId)
                .ToListAsync();
        }

        public async Task<int> GetIncidentCountByMissionIdAsync(int missionId)
        {
            return await _context.Incidents
                .AsNoTracking()
                .CountAsync(i => i.MissionId == missionId);
        }

        public IQueryable<Incident> GetQueryable()
        {
            return _context.Incidents.AsQueryable();
        }
    }
}