using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Application.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Application.DTOs.User;

namespace gestionMissionBack.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IValidator<UserDto> _userDtoValidator;
        private readonly IValidator<UserUpdateDto> _userUpdateDtoValidator;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IValidator<UserDto> userDtoValidator,
            IValidator<UserUpdateDto> userUpdateDtoValidator,
            IMapper mapper,
            IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _userDtoValidator = userDtoValidator ?? throw new ArgumentNullException(nameof(userDtoValidator));
            _userUpdateDtoValidator = userUpdateDtoValidator ?? throw new ArgumentNullException(nameof(userUpdateDtoValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }


        public async Task<PagedResult<UserDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var users = await _userRepository.GetPagedAsync(pageNumber, pageSize);

            return new PagedResult<UserDto>
            {
                Data = _mapper.Map<IEnumerable<UserDto>>(users.Data),
                TotalRecords = users.TotalRecords
            };
        }


        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUserAsync(UserDto userDto)
        {
            // Validate DTO
            var validationResult = await _userDtoValidator.ValidateAsync(userDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Check if email already exists
            var existingUser = await _userRepository.GetUserByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            // Resolve RoleId
            var role = await _roleRepository.FindByNameAsync(userDto.Role);
            // Validator ensures role exists, so role should not be null

            var user = _mapper.Map<User>(userDto);
            user.RoleId = role.RoleId;
            user.PasswordHash = PasswordService.HashPassword(userDto.PasswordHash);

            var createdUser = await _userRepository.AddAsync(user);
            return _mapper.Map<UserDto>(createdUser);
        }

        public async Task<bool> UpdateUserAsync(UserUpdateDto userDto)
        {
            // Validate DTO
            var validationResult = await _userUpdateDtoValidator.ValidateAsync(userDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingUser = await _userRepository.GetByIdAsync(userDto.UserId);
            if (existingUser == null)
            {
                return false;
            }

            // Resolve RoleId if Role is provided
            if (!string.IsNullOrWhiteSpace(userDto.Role))
            {
                var role = await _roleRepository.FindByNameAsync(userDto.Role);
                // Validator ensures role exists
                existingUser.RoleId = role.RoleId;
            }

            // Map DTO to existing user
            _mapper.Map(userDto, existingUser);

            // Hash password only if a new one is provided
            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                existingUser.PasswordHash = PasswordService.HashPassword(userDto.Password);
            }

            return await _userRepository.UpdateAsync(existingUser);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null || !PasswordService.VerifyPassword(password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}