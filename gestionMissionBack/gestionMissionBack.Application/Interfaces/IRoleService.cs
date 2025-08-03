using gestionMissionBack.Application.DTOs.Role;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(int id);
        Task<RoleDto?> GetRoleByNameAsync(string roleName);
        Task<RoleDto> CreateRoleAsync(RoleDto roleDto);
        Task UpdateRoleAsync(RoleDto roleDto);
        Task DeleteRoleAsync(int id);
        Task<PagedResult<RoleDto>> GetPagedAsync(int pageNumber, int pageSize);

    }
}
