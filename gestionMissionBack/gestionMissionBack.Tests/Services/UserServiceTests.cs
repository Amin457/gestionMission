using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using gestionMissionBack.Application.DTOs.User;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Services;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Application.Utils;
using gestionMissionBack.Domain.Helpers;
using FluentValidation;
using FluentValidation.Results;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using gestionMissionBack.Infrastructure.Interfaces;

namespace gestionMissionBack.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IValidator<UserDto>> _mockUserDtoValidator;
        private readonly Mock<IValidator<UserUpdateDto>> _mockUserUpdateDtoValidator;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockUserDtoValidator = new Mock<IValidator<UserDto>>();
            _mockUserUpdateDtoValidator = new Mock<IValidator<UserUpdateDto>>();
            _mockMapper = new Mock<IMapper>();
            _mockConfiguration = new Mock<IConfiguration>();

            _userService = new UserService(
                _mockUserRepository.Object,
                _mockRoleRepository.Object,
                _mockUserDtoValidator.Object,
                _mockUserUpdateDtoValidator.Object,
                _mockMapper.Object,
                _mockConfiguration.Object
            );
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnMappedUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" },
                new User { UserId = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
            };

            var userDtos = new List<UserDto>
            {
                new UserDto { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" },
                new UserDto { UserId = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
            };

            _mockUserRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(users);
            _mockMapper.Setup(x => x.Map<IEnumerable<UserDto>>(users)).Returns(userDtos);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockUserRepository.Verify(x => x.GetAllAsync(), Times.Once);
            _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Once);
        }

        [Fact]
        public async Task GetPagedAsync_ShouldReturnPagedUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" }
            };

            var userDtos = new List<UserDto>
            {
                new UserDto { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" }
            };

            var pagedResult = new PagedResult<User>
            {
                Data = users,
                TotalRecords = 1
            };

            _mockUserRepository.Setup(x => x.GetPagedAsync(1, 10)).ReturnsAsync(pagedResult);
            _mockMapper.Setup(x => x.Map<IEnumerable<UserDto>>(users)).Returns(userDtos);

            // Act
            var result = await _userService.GetPagedAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalRecords);
            Assert.Single(result.Data);
            _mockUserRepository.Verify(x => x.GetPagedAsync(1, 10), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
            var userDto = new UserDto { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" };

            _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _mockMapper.Setup(x => x.Map<UserDto>(user)).Returns(userDto);

            // Act
            var result = await _userService.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            _mockUserRepository.Verify(x => x.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUserRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User)null);
            _mockMapper.Setup(x => x.Map<UserDto>((User)null)).Returns((UserDto)null);

            // Act
            var result = await _userService.GetUserByIdAsync(999);

            // Assert
            Assert.Null(result);
            _mockUserRepository.Verify(x => x.GetByIdAsync(999), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser_WhenValidData()
        {
            // Arrange
            var userDto = new UserDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                PasswordHash = "password123",
                Role = "Driver"
            };

            var role = new Role { RoleId = 1, Name = "Driver" };
            var user = new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
            var createdUser = new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
            var createdUserDto = new UserDto { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" };

            var validationResult = new ValidationResult();
            _mockUserDtoValidator.Setup(x => x.ValidateAsync(userDto, default)).ReturnsAsync(validationResult);
            _mockUserRepository.Setup(x => x.GetUserByEmailAsync("john@test.com")).ReturnsAsync((User)null);
            _mockRoleRepository.Setup(x => x.FindByNameAsync("Driver")).ReturnsAsync(role);
            _mockMapper.Setup(x => x.Map<User>(userDto)).Returns(user);
            _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>())).ReturnsAsync(createdUser);
            _mockMapper.Setup(x => x.Map<UserDto>(createdUser)).Returns(createdUserDto);

            // Act
            var result = await _userService.CreateUserAsync(userDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowValidationException_WhenInvalidData()
        {
            // Arrange
            var userDto = new UserDto { Email = "invalid-email" };
            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Email", "Invalid email format")
            });

            _mockUserDtoValidator.Setup(x => x.ValidateAsync(userDto, default)).ReturnsAsync(validationResult);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _userService.CreateUserAsync(userDto));
            _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowInvalidOperationException_WhenEmailExists()
        {
            // Arrange
            var userDto = new UserDto { Email = "existing@test.com" };
            var existingUser = new User { UserId = 1, Email = "existing@test.com" };

            var validationResult = new ValidationResult();
            _mockUserDtoValidator.Setup(x => x.ValidateAsync(userDto, default)).ReturnsAsync(validationResult);
            _mockUserRepository.Setup(x => x.GetUserByEmailAsync("existing@test.com")).ReturnsAsync(existingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(userDto));
            Assert.Equal("A user with this email already exists.", exception.Message);
            _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenValidData()
        {
            // Arrange
            var userUpdateDto = new UserUpdateDto
            {
                UserId = 1,
                FirstName = "John Updated",
                LastName = "Doe Updated",
                Email = "john.updated@test.com",
                Role = "Driver"
            };

            var existingUser = new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
            var role = new Role { RoleId = 1, Name = "Driver" };

            var validationResult = new ValidationResult();
            _mockUserUpdateDtoValidator.Setup(x => x.ValidateAsync(userUpdateDto, default)).ReturnsAsync(validationResult);
            _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingUser);
            _mockRoleRepository.Setup(x => x.FindByNameAsync("Driver")).ReturnsAsync(role);
            _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(true);

            // Act
            var result = await _userService.UpdateUserAsync(userUpdateDto);

            // Assert
            Assert.True(result);
            _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var userUpdateDto = new UserUpdateDto { UserId = 999 };

            var validationResult = new ValidationResult();
            _mockUserUpdateDtoValidator.Setup(x => x.ValidateAsync(userUpdateDto, default)).ReturnsAsync(validationResult);
            _mockUserRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.UpdateUserAsync(userUpdateDto);

            // Assert
            Assert.False(result);
            _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            _mockUserRepository.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUserAsync(1);

            // Assert
            Assert.True(result);
            _mockUserRepository.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUserRepository.Setup(x => x.DeleteAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _userService.DeleteUserAsync(999);

            // Assert
            Assert.False(result);
            _mockUserRepository.Verify(x => x.DeleteAsync(999), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenValidCredentials()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                PasswordHash = PasswordService.HashPassword("password123"),
                Role = new Role { Name = "Driver" }
            };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync("john@test.com")).ReturnsAsync(user);
            _mockConfiguration.Setup(x => x["Jwt:Secret"]).Returns("your-secret-key-here-make-it-long-enough");
            _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("your-issuer");
            _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("your-audience");

            // Act
            var result = await _userService.LoginAsync("john@test.com", "password123");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            
            // Verify it's a valid JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(result);
            Assert.NotNull(token);
            
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync("john@test.com"), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenInvalidEmail()
        {
            // Arrange
            _mockUserRepository.Setup(x => x.GetUserByEmailAsync("nonexistent@test.com")).ReturnsAsync((User)null);

            // Act
            var result = await _userService.LoginAsync("nonexistent@test.com", "password123");

            // Assert
            Assert.Null(result);
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync("nonexistent@test.com"), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenInvalidPassword()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Email = "john@test.com",
                PasswordHash = PasswordService.HashPassword("correctpassword")
            };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync("john@test.com")).ReturnsAsync(user);

            // Act
            var result = await _userService.LoginAsync("john@test.com", "wrongpassword");

            // Assert
            Assert.Null(result);
            _mockUserRepository.Verify(x => x.GetUserByEmailAsync("john@test.com"), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenDependenciesAreNull()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UserService(null, _mockRoleRepository.Object, _mockUserDtoValidator.Object, _mockUserUpdateDtoValidator.Object, _mockMapper.Object, _mockConfiguration.Object));
            Assert.Throws<ArgumentNullException>(() => new UserService(_mockUserRepository.Object, null, _mockUserDtoValidator.Object, _mockUserUpdateDtoValidator.Object, _mockMapper.Object, _mockConfiguration.Object));
            Assert.Throws<ArgumentNullException>(() => new UserService(_mockUserRepository.Object, _mockRoleRepository.Object, null, _mockUserUpdateDtoValidator.Object, _mockMapper.Object, _mockConfiguration.Object));
            Assert.Throws<ArgumentNullException>(() => new UserService(_mockUserRepository.Object, _mockRoleRepository.Object, _mockUserDtoValidator.Object, null, _mockMapper.Object, _mockConfiguration.Object));
            Assert.Throws<ArgumentNullException>(() => new UserService(_mockUserRepository.Object, _mockRoleRepository.Object, _mockUserDtoValidator.Object, _mockUserUpdateDtoValidator.Object, null, _mockConfiguration.Object));
            Assert.Throws<ArgumentNullException>(() => new UserService(_mockUserRepository.Object, _mockRoleRepository.Object, _mockUserDtoValidator.Object, _mockUserUpdateDtoValidator.Object, _mockMapper.Object, null));
        }
    }
}
