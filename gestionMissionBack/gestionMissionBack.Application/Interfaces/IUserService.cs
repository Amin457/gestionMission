using gestionMissionBack.Application.DTOs.User;
using gestionMissionBack.Application.Utils;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(int id);
        Task<UserDto> CreateUserAsync(UserDto userDto);
        Task<bool> UpdateUserAsync(UserUpdateDto userDto);
        Task<bool> DeleteUserAsync(int id);
        Task<string> LoginAsync(string email, string password);
        Task<PagedResult<UserDto>> GetPagedAsync(int pageNumber, int pageSize);

    }
}
