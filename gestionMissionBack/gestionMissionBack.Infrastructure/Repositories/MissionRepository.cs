using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Infrastructure.Repositories
{
    public class MissionRepository : GenericRepository<Mission>, IMissionRepository
    {
        private readonly MissionFleetContext _context;

        public MissionRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<PagedResult<Mission>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Missions
                .Include(m => m.Requester)
                .Include(m => m.Driver)
                .AsQueryable();

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(m => m.SystemDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Mission>
            {
                Data = data,
                TotalRecords = totalRecords
            };
        }
    }
}