using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.DTOs.Role;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IValidator<RoleDto> _roleValidator;
        private readonly IMapper _mapper;

        public RoleService(
            IRoleRepository roleRepository,
            IValidator<RoleDto> roleValidator,
            IMapper mapper)
        {
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _roleValidator = roleValidator ?? throw new ArgumentNullException(nameof(roleValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<RoleDto> GetRoleByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            return _mapper.Map<RoleDto>(role);
        }

        public async Task<PagedResult<RoleDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var roles = await _roleRepository.GetPagedAsync(pageNumber, pageSize);

            return new PagedResult<RoleDto>
            {
                Data = _mapper.Map<IEnumerable<RoleDto>>(roles.Data),
                TotalRecords = roles.TotalRecords
            };
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }

        public async Task<RoleDto> GetRoleByNameAsync(string roleName)
        {
            var role = await _roleRepository.FindByNameAsync(roleName);
            return _mapper.Map<RoleDto>(role);
        }

        public async Task<RoleDto> CreateRoleAsync(RoleDto roleDto)
        {
            var validationResult = await _roleValidator.ValidateAsync(roleDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Check if role already exists
            var existingRole = await _roleRepository.FindByNameAsync(roleDto.Name);
            if (existingRole != null)
            {
                throw new InvalidOperationException("A role with this name already exists.");
            }
            var role = _mapper.Map<Role>(roleDto);
            var createdRole = await _roleRepository.AddAsync(role);
            return _mapper.Map<RoleDto>(createdRole);
        }

        public async Task UpdateRoleAsync(RoleDto roleDto)
        {
            var validationResult = await _roleValidator.ValidateAsync(roleDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingRole = await _roleRepository.GetByIdAsync(roleDto.RoleId);
            if (existingRole == null)
            {
                throw new KeyNotFoundException($"Role with ID {roleDto.RoleId} not found.");
            }

            _mapper.Map(roleDto, existingRole);
            var updated = await _roleRepository.UpdateAsync(existingRole);
            if (!updated)
            {
                throw new Exception($"Failed to update role with ID {roleDto.RoleId}");
            }
        }

        public async Task DeleteRoleAsync(int id)
        {
            var deleted = await _roleRepository.DeleteAsync(id);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Role with ID {id} not found or could not be deleted.");
            }
        }
    }
}