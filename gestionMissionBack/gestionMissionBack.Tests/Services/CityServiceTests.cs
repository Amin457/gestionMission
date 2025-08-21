using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using gestionMissionBack.Application.DTOs.City;
using gestionMissionBack.Application.Services;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Infrastructure.Interfaces;
using Moq;
using Xunit;

namespace gestionMissionBack.Tests.Services
{
    public class CityServiceTests
    {
        private readonly Mock<ICityRepository> _mockCityRepository;
        private readonly Mock<IValidator<CityDto>> _mockValidator;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CityService _cityService;

        public CityServiceTests()
        {
            _mockCityRepository = new Mock<ICityRepository>();
            _mockValidator = new Mock<IValidator<CityDto>>();
            _mockMapper = new Mock<IMapper>();
            _cityService = new CityService(_mockCityRepository.Object, _mockValidator.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetCityByIdAsync_WithValidId_ReturnsCityDto()
        {
            // Arrange
            var cityId = 1;
            var city = new City { CityId = cityId, Name = "Test City", Region = "Test Region" };
            var cityDto = new CityDto { CityId = cityId, Name = "Test City", Region = "Test Region" };

            _mockCityRepository.Setup(x => x.GetByIdAsync(cityId)).ReturnsAsync(city);
            _mockMapper.Setup(x => x.Map<CityDto>(city)).Returns(cityDto);

            // Act
            var result = await _cityService.GetCityByIdAsync(cityId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cityId, result.CityId);
            Assert.Equal("Test City", result.Name);
            _mockCityRepository.Verify(x => x.GetByIdAsync(cityId), Times.Once);
        }

        [Fact]
        public async Task GetAllCitiesAsync_ReturnsAllCities()
        {
            // Arrange
            var cities = new List<City>
            {
                new City { CityId = 1, Name = "City 1", Region = "Region 1" },
                new City { CityId = 2, Name = "City 2", Region = "Region 2" }
            };

            var cityDtos = cities.Select(c => new CityDto { CityId = c.CityId, Name = c.Name, Region = c.Region }).ToList();

            _mockCityRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(cities);
            _mockMapper.Setup(x => x.Map<IEnumerable<CityDto>>(cities)).Returns(cityDtos);

            // Act
            var result = await _cityService.GetAllCitiesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockCityRepository.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateCityAsync_WithValidCity_ReturnsCreatedCity()
        {
            // Arrange
            var cityDto = new CityDto { Name = "New City", Region = "New Region" };
            var city = new City { CityId = 1, Name = "New City", Region = "New Region" };
            var createdCity = new City { CityId = 1, Name = "New City", Region = "New Region" };

            var validationResult = new ValidationResult();
            _mockValidator.Setup(x => x.ValidateAsync(cityDto, default)).ReturnsAsync(validationResult);
            _mockCityRepository.Setup(x => x.FindByNameAsync(cityDto.Name)).ReturnsAsync((City)null);
            _mockMapper.Setup(x => x.Map<City>(cityDto)).Returns(city);
            _mockCityRepository.Setup(x => x.AddAsync(city)).ReturnsAsync(createdCity);
            _mockMapper.Setup(x => x.Map<CityDto>(createdCity)).Returns(cityDto);

            // Act
            var result = await _cityService.CreateCityAsync(cityDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New City", result.Name);
            _mockCityRepository.Verify(x => x.AddAsync(city), Times.Once);
        }

        [Fact]
        public async Task CreateCityAsync_WithExistingCityName_ThrowsException()
        {
            // Arrange
            var cityDto = new CityDto { Name = "Existing City", Region = "Region" };
            var existingCity = new City { CityId = 1, Name = "Existing City", Region = "Region" };

            var validationResult = new ValidationResult();
            _mockValidator.Setup(x => x.ValidateAsync(cityDto, default)).ReturnsAsync(validationResult);
            _mockCityRepository.Setup(x => x.FindByNameAsync(cityDto.Name)).ReturnsAsync(existingCity);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _cityService.CreateCityAsync(cityDto));
            _mockCityRepository.Verify(x => x.AddAsync(It.IsAny<City>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCityAsync_WithValidId_DeletesCity()
        {
            // Arrange
            var cityId = 1;
            _mockCityRepository.Setup(x => x.DeleteAsync(cityId)).ReturnsAsync(true);

            // Act
            await _cityService.DeleteCityAsync(cityId);

            // Assert
            _mockCityRepository.Verify(x => x.DeleteAsync(cityId), Times.Once);
        }

        [Fact]
        public async Task DeleteCityAsync_WithInvalidId_ThrowsException()
        {
            // Arrange
            var cityId = 999;
            _mockCityRepository.Setup(x => x.DeleteAsync(cityId)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _cityService.DeleteCityAsync(cityId));
            _mockCityRepository.Verify(x => x.DeleteAsync(cityId), Times.Once);
        }
    }
}
