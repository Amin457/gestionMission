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
    public class VehicleReservationRepository : GenericRepository<VehicleReservation>, IVehicleReservationRepository
    {
        private readonly MissionFleetContext _context;

        public VehicleReservationRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<VehicleReservation> GetQueryable()
        {
            return _context.VehicleReservations.AsNoTracking();
        }

        public async Task<IEnumerable<VehicleReservation>> GetReservationsByVehicleIdAsync(int vehicleId)
        {
            return await _context.VehicleReservations
                .AsNoTracking()
                .Where(vr => vr.VehicleId == vehicleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<VehicleReservation>> GetReservationsByRequesterIdAsync(int requesterId)
        {
            return await _context.VehicleReservations
                .AsNoTracking()
                .Where(vr => vr.RequesterId == requesterId)
                .ToListAsync();
        }

        public async Task<IEnumerable<VehicleReservation>> GetReservationsByStatusAsync(string status)
        {
            return await _context.VehicleReservations
                .AsNoTracking()
                .Where(vr => vr.Status.ToString() == status)
                .ToListAsync();
        }
    }
}