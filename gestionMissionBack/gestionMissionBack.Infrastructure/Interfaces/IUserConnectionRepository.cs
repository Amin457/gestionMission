using gestionMissionBack.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface IUserConnectionRepository : IGenericRepository<UserConnection>
    {
        Task<IEnumerable<UserConnection>> GetUserConnectionsAsync(int userId);
        Task<UserConnection?> GetByConnectionIdAsync(string connectionId);
        Task<bool> RemoveConnectionAsync(string connectionId);
        Task<bool> UpdateLastActivityAsync(string connectionId);
        Task<bool> CleanupInactiveConnectionsAsync(TimeSpan timeout);
    }
} 