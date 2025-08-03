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
    public class UserConnectionRepository : GenericRepository<UserConnection>, IUserConnectionRepository
    {
        public UserConnectionRepository(MissionFleetContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserConnection>> GetUserConnectionsAsync(int userId)
        {
            return await _context.UserConnections
                .Where(uc => uc.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserConnection?> GetByConnectionIdAsync(string connectionId)
        {
            return await _context.UserConnections
                .FirstOrDefaultAsync(uc => uc.ConnectionId == connectionId);
        }

        public async Task<bool> RemoveConnectionAsync(string connectionId)
        {
            var connection = await _context.UserConnections
                .FirstOrDefaultAsync(uc => uc.ConnectionId == connectionId);

            if (connection == null)
                return false;

            _context.UserConnections.Remove(connection);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLastActivityAsync(string connectionId)
        {
            var connection = await _context.UserConnections
                .FirstOrDefaultAsync(uc => uc.ConnectionId == connectionId);

            if (connection == null)
                return false;

            connection.LastActivity = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CleanupInactiveConnectionsAsync(TimeSpan timeout)
        {
            var cutoffTime = DateTime.UtcNow.Subtract(timeout);
            var inactiveConnections = await _context.UserConnections
                .Where(uc => uc.LastActivity < cutoffTime)
                .ToListAsync();

            _context.UserConnections.RemoveRange(inactiveConnections);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 