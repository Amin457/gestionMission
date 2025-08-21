using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using gestionMissionBack.Application.DTOs.Route;
using gestionMissionBack.Application.Services;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Infrastructure.Interfaces;
using Moq;
using Xunit;

namespace gestionMissionBack.Tests.Services
{
    public class RouteServiceTests
    {
        private readonly Mock<IRouteRepository> _mockRouteRepository;
        private readonly Mock<IValidator<RouteDto>> _mockValidator;
        private readonly Mock<IMapper> _mockMapper;
        private readonly RouteService _routeService;

        public RouteServiceTests()
        {
            _mockRouteRepository = new Mock<IRouteRepository>();
            _mockValidator = new Mock<IValidator<RouteDto>>();
            _mockMapper = new Mock<IMapper>();
            _routeService = new RouteService(_mockRouteRepository.Object, _mockValidator.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetRouteByIdAsync_WithValidId_ReturnsRouteDto()
        {
            // Arrange
            var routeId = 1;
            var route = new Route { RouteId = routeId, CircuitId = 1, DepartureSiteId = 1, ArrivalSiteId = 2, DistanceKm = 100.5, Ordre = 1 };
            var routeDto = new RouteDto { RouteId = routeId, CircuitId = 1, DepartureSiteId = 1, ArrivalSiteId = 2, DistanceKm = 100.5, Ordre = 1 };

            _mockRouteRepository.Setup(x => x.GetByIdAsync(routeId)).ReturnsAsync(route);
            _mockMapper.Setup(x => x.Map<RouteDto>(route)).Returns(routeDto);

            // Act
            var result = await _routeService.GetRouteByIdAsync(routeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(routeId, result.RouteId);
            Assert.Equal(100.5, result.DistanceKm);
            _mockRouteRepository.Verify(x => x.GetByIdAsync(routeId), Times.Once);
        }

        [Fact]
        public async Task GetAllRoutesAsync_ReturnsAllRoutes()
        {
            // Arrange
            var routes = new List<Route>
            {
                new Route { RouteId = 1, CircuitId = 1, DepartureSiteId = 1, ArrivalSiteId = 2, DistanceKm = 50.0, Ordre = 1 },
                new Route { RouteId = 2, CircuitId = 1, DepartureSiteId = 2, ArrivalSiteId = 3, DistanceKm = 75.5, Ordre = 2 },
                new Route { RouteId = 3, CircuitId = 2, DepartureSiteId = 1, ArrivalSiteId = 4, DistanceKm = 120.0, Ordre = 1 }
            };

            var routeDtos = routes.Select(r => new RouteDto 
            { 
                RouteId = r.RouteId, 
                CircuitId = r.CircuitId, 
                DepartureSiteId = r.DepartureSiteId, 
                ArrivalSiteId = r.ArrivalSiteId, 
                DistanceKm = r.DistanceKm, 
                Ordre = r.Ordre 
            }).ToList();

            _mockRouteRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(routes);
            _mockMapper.Setup(x => x.Map<IEnumerable<RouteDto>>(routes)).Returns(routeDtos);

            // Act
            var result = await _routeService.GetAllRoutesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            _mockRouteRepository.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetRoutesByCircuitIdAsync_WithValidCircuitId_ReturnsRoutes()
        {
            // Arrange
            var circuitId = 1;
            var routes = new List<Route>
            {
                new Route { RouteId = 1, CircuitId = circuitId, DepartureSiteId = 1, ArrivalSiteId = 2, DistanceKm = 50.0, Ordre = 1 },
                new Route { RouteId = 2, CircuitId = circuitId, DepartureSiteId = 2, ArrivalSiteId = 3, DistanceKm = 75.5, Ordre = 2 }
            };

            var routeDtos = routes.Select(r => new RouteDto 
            { 
                RouteId = r.RouteId, 
                CircuitId = r.CircuitId, 
                DepartureSiteId = r.DepartureSiteId, 
                ArrivalSiteId = r.ArrivalSiteId, 
                DistanceKm = r.DistanceKm, 
                Ordre = r.Ordre 
            }).ToList();

            _mockRouteRepository.Setup(x => x.GetRoutesByCircuitIdAsync(circuitId)).ReturnsAsync(routes);
            _mockMapper.Setup(x => x.Map<IEnumerable<RouteDto>>(routes)).Returns(routeDtos);

            // Act
            var result = await _routeService.GetRoutesByCircuitIdAsync(circuitId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal(circuitId, r.CircuitId));
            _mockRouteRepository.Verify(x => x.GetRoutesByCircuitIdAsync(circuitId), Times.Once);
        }

        [Fact]
        public async Task GetTotalDistanceByCircuitIdAsync_WithValidCircuitId_ReturnsTotalDistance()
        {
            // Arrange
            var circuitId = 1;
            var expectedDistance = 125.5;
            _mockRouteRepository.Setup(x => x.GetTotalDistanceByCircuitIdAsync(circuitId)).ReturnsAsync(expectedDistance);

            // Act
            var result = await _routeService.GetTotalDistanceByCircuitIdAsync(circuitId);

            // Assert
            Assert.Equal(expectedDistance, result);
            _mockRouteRepository.Verify(x => x.GetTotalDistanceByCircuitIdAsync(circuitId), Times.Once);
        }

        [Fact]
        public async Task CreateRouteAsync_WithValidRoute_ReturnsCreatedRoute()
        {
            // Arrange
            var routeDto = new RouteDto { CircuitId = 1, DepartureSiteId = 1, ArrivalSiteId = 2, DistanceKm = 200.0, Ordre = 1 };
            var route = new Route { RouteId = 1, CircuitId = 1, DepartureSiteId = 1, ArrivalSiteId = 2, DistanceKm = 200.0, Ordre = 1 };
            var createdRoute = new Route { RouteId = 1, CircuitId = 1, DepartureSiteId = 1, ArrivalSiteId = 2, DistanceKm = 200.0, Ordre = 1 };

            var validationResult = new ValidationResult();
            _mockValidator.Setup(x => x.ValidateAsync(routeDto, default)).ReturnsAsync(validationResult);
            _mockMapper.Setup(x => x.Map<Route>(routeDto)).Returns(route);
            _mockRouteRepository.Setup(x => x.AddAsync(route)).ReturnsAsync(createdRoute);
            _mockMapper.Setup(x => x.Map<RouteDto>(createdRoute)).Returns(routeDto);

            // Act
            var result = await _routeService.CreateRouteAsync(routeDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200.0, result.DistanceKm);
            Assert.Equal(1, result.CircuitId);
            _mockRouteRepository.Verify(x => x.AddAsync(route), Times.Once);
        }

        [Fact]
        public async Task UpdateRouteAsync_WithValidRoute_UpdatesRoute()
        {
            // Arrange
            var routeDto = new RouteDto { RouteId = 1, CircuitId = 1, DepartureSiteId = 1, ArrivalSiteId = 2, DistanceKm = 150.0, Ordre = 1 };
            var existingRoute = new Route { RouteId = 1, CircuitId = 1, DepartureSiteId = 1, ArrivalSiteId = 2, DistanceKm = 100.0, Ordre = 1 };

            var validationResult = new ValidationResult();
            _mockValidator.Setup(x => x.ValidateAsync(routeDto, default)).ReturnsAsync(validationResult);
            _mockRouteRepository.Setup(x => x.GetByIdAsync(routeDto.RouteId)).ReturnsAsync(existingRoute);
            _mockRouteRepository.Setup(x => x.UpdateAsync(It.IsAny<Route>())).ReturnsAsync(true);

            // Act
            await _routeService.UpdateRouteAsync(routeDto);

            // Assert
            _mockRouteRepository.Verify(x => x.UpdateAsync(It.IsAny<Route>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRouteAsync_WithValidId_DeletesRoute()
        {
            // Arrange
            var routeId = 1;
            _mockRouteRepository.Setup(x => x.DeleteAsync(routeId)).ReturnsAsync(true);

            // Act
            await _routeService.DeleteRouteAsync(routeId);

            // Assert
            _mockRouteRepository.Verify(x => x.DeleteAsync(routeId), Times.Once);
        }

        [Fact]
        public async Task DeleteRouteAsync_WithInvalidId_ThrowsException()
        {
            // Arrange
            var routeId = 999;
            _mockRouteRepository.Setup(x => x.DeleteAsync(routeId)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _routeService.DeleteRouteAsync(routeId));
            _mockRouteRepository.Verify(x => x.DeleteAsync(routeId), Times.Once);
        }
    }
}
