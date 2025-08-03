using gestionMissionBack.Domain.Entities;
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
    public class RoleRepository : GenericRepository<Role> , IRoleRepository
    {
        private readonly MissionFleetContext _context;

        public RoleRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Role?> FindByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
        }
    }
}
