using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using gestionMissionBack.Application.DTOs.User;
using gestionMissionBack.Application.Services;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Infrastructure.Interfaces;
using Moq;
using Xunit;

namespace gestionMissionBack.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IValidator<UserDto>> _mockUserDtoValidator;
        private readonly Mock<IValidator<UserUpdateDto>> _mockUserUpdateDtoValidator;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _mockConfiguration;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockUserDtoValidator = new Mock<IValidator<UserDto>>();
            _mockUserUpdateDtoValidator = new Mock<IValidator<UserUpdateDto>>();
            _mockMapper = new Mock<IMapper>();
            _mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            _userService = new UserService(
                _mockUserRepository.Object, 
                _mockRoleRepository.Object, 
                _mockUserDtoValidator.Object, 
                _mockUserUpdateDtoValidator.Object, 
                _mockMapper.Object, 
                _mockConfiguration.Object);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
                new User { UserId = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
            };

            var userDtos = users.Select(u => new UserDto 
            { 
                UserId = u.UserId, 
                FirstName = u.FirstName, 
                LastName = u.LastName, 
                Email = u.Email 
            }).ToList();

            _mockUserRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(users);
            _mockMapper.Setup(x => x.Map<IEnumerable<UserDto>>(users)).Returns(userDtos);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockUserRepository.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ReturnsUserDto()
        {
            // Arrange
            var userId = 1;
            var user = new User { UserId = userId, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
            var userDto = new UserDto { UserId = userId, FirstName = "John", LastName = "Doe", Email = "john@example.com" };

            _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockMapper.Setup(x => x.Map<UserDto>(user)).Returns(userDto);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var userId = 999;
            _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result);
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WithValidUser_ReturnsCreatedUser()
        {
            // Arrange
            var userDto = new UserDto 
            { 
                FirstName = "New", 
                LastName = "User", 
                Email = "newuser@example.com", 
                PasswordHash = "password123",
                Role = "Driver"
            };
            
            var role = new Role { RoleId = 1, Name = "Driver" };
            var user = new User { UserId = 1, FirstName = "New", LastName = "User", Email = "newuser@example.com" };
            var createdUser = new User { UserId = 1, FirstName = "New", LastName = "User", Email = "newuser@example.com" };

            var validationResult = new ValidationResult();
            _mockUserDtoValidator.Setup(x => x.ValidateAsync(userDto, default)).ReturnsAsync(validationResult);
            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(userDto.Email)).ReturnsAsync((User)null);
            _mockRoleRepository.Setup(x => x.FindByNameAsync(userDto.Role)).ReturnsAsync(role);
            _mockMapper.Setup(x => x.Map<User>(userDto)).Returns(user);
            _mockUserRepository.Setup(x => x.AddAsync(user)).ReturnsAsync(createdUser);
            _mockMapper.Setup(x => x.Map<UserDto>(createdUser)).Returns(userDto);

            // Act
            var result = await _userService.CreateUserAsync(userDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New", result.FirstName);
            Assert.Equal("User", result.LastName);
            _mockUserRepository.Verify(x => x.AddAsync(user), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WithExistingEmail_ThrowsException()
        {
            // Arrange
            var userDto = new UserDto 
            { 
                FirstName = "Existing", 
                LastName = "User", 
                Email = "existing@example.com", 
                PasswordHash = "password123",
                Role = "Driver"
            };
            
            var existingUser = new User { UserId = 1, Email = "existing@example.com" };

            var validationResult = new ValidationResult();
            _mockUserDtoValidator.Setup(x => x.ValidateAsync(userDto, default)).ReturnsAsync(validationResult);
            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(userDto.Email)).ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(userDto));
            _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetPagedAsync_WithValidParameters_ReturnsPagedResult()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var users = new List<User>
            {
                new User { UserId = 1, FirstName = "John", LastName = "Doe" },
                new User { UserId = 2, FirstName = "Jane", LastName = "Smith" }
            };

            var userDtos = users.Select(u => new UserDto 
            { 
                UserId = u.UserId, 
                FirstName = u.FirstName, 
                LastName = u.LastName 
            }).ToList();

            var pagedUsers = new PagedResult<User>
            {
                Data = users,
                TotalRecords = 2
            };

            var expectedPagedResult = new PagedResult<UserDto>
            {
                Data = userDtos,
                TotalRecords = 2
            };

            _mockUserRepository.Setup(x => x.GetPagedAsync(pageNumber, pageSize)).ReturnsAsync(pagedUsers);
            _mockMapper.Setup(x => x.Map<IEnumerable<UserDto>>(users)).Returns(userDtos);

            // Act
            var result = await _userService.GetPagedAsync(pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalRecords);
            Assert.Equal(2, result.Data.Count());
            _mockUserRepository.Verify(x => x.GetPagedAsync(pageNumber, pageSize), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithValidUser_ReturnsTrue()
        {
            // Arrange
            var userUpdateDto = new UserUpdateDto 
            { 
                UserId = 1, 
                FirstName = "Updated", 
                LastName = "User", 
                Email = "updated@example.com" 
            };

            var existingUser = new User { UserId = 1, FirstName = "Old", LastName = "User", Email = "old@example.com" };

            var validationResult = new ValidationResult();
            _mockUserUpdateDtoValidator.Setup(x => x.ValidateAsync(userUpdateDto, default)).ReturnsAsync(validationResult);
            _mockUserRepository.Setup(x => x.GetByIdAsync(userUpdateDto.UserId)).ReturnsAsync(existingUser);
            _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(true);

            // Act
            var result = await _userService.UpdateUserAsync(userUpdateDto);

            // Assert
            Assert.True(result);
            _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithInvalidUserId_ReturnsFalse()
        {
            // Arrange
            var userUpdateDto = new UserUpdateDto 
            { 
                UserId = 999, 
                FirstName = "Updated", 
                LastName = "User", 
                Email = "updated@example.com" 
            };

            var validationResult = new ValidationResult();
            _mockUserUpdateDtoValidator.Setup(x => x.ValidateAsync(userUpdateDto, default)).ReturnsAsync(validationResult);
            _mockUserRepository.Setup(x => x.GetByIdAsync(userUpdateDto.UserId)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.UpdateUserAsync(userUpdateDto);

            // Assert
            Assert.False(result);
            _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
