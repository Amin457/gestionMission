using gestionMissionBack.Domain.Entities;

namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> FindByNameAsync(string roleName);
    }
}
