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
    public class RouteRepository : GenericRepository<Route>, IRouteRepository
    {
        private readonly MissionFleetContext _context;

        public RouteRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Route>> GetRoutesByCircuitIdAsync(int circuitId)
        {
            return await _context.Routes
                .AsNoTracking()
                .Where(r => r.CircuitId == circuitId)
                .ToListAsync();
        }

        public async Task<double> GetTotalDistanceByCircuitIdAsync(int circuitId)
        {
            return (double)await _context.Routes
                .AsNoTracking()
                .Where(r => r.CircuitId == circuitId)
                .SumAsync(r => r.DistanceKm);
        }
    }
}