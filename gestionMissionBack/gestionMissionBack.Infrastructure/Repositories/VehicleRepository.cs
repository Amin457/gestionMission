using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Persistence;
using gestionMissionBack.Domain.DTOs;
using gestionMissionBack.Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestionMissionBack.Infrastructure.Repositories
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        private readonly MissionFleetContext _context;

        public VehicleRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await _context.Vehicles
                .AsNoTracking()
                .Where(v => v.Availability)
                .ToListAsync();
        }

        public async Task<int> GetVehicleCountByTypeAsync(VehicleType type)
        {
            return await _context.Vehicles
                .AsNoTracking()
                .CountAsync(v => v.Type == type);
        }

        public async Task<PagedResult<Vehicle>> GetPagedAsync(int pageNumber, int pageSize, VehicleFilter filter = null)
        {
            var query = _context.Vehicles.AsNoTracking();

            // Apply filters if provided
            if (filter != null)
            {
                if (filter.Type.HasValue)
                    query = query.Where(v => v.Type == filter.Type.Value);

                if (filter.Availability.HasValue)
                    query = query.Where(v => v.Availability == filter.Availability.Value);

                if (!string.IsNullOrEmpty(filter.LicensePlate))
                    query = query.Where(v => v.LicensePlate.Contains(filter.LicensePlate));

                if (filter.MinCapacity.HasValue)
                    query = query.Where(v => v.MaxCapacity >= filter.MinCapacity.Value);

                if (filter.MaxCapacity.HasValue)
                    query = query.Where(v => v.MaxCapacity <= filter.MaxCapacity.Value);
            }

            var totalRecords = await query.CountAsync();

            var vehicles = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Vehicle>
            {
                Data = vehicles,
                TotalRecords = totalRecords
            };
        }
    }
}