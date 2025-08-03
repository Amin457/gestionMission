using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using gestionMissionBack.Application.DTOs.Mission;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Services;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Application.DTOs.Notification;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using gestionMissionBack.Infrastructure.Interfaces;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore.Query;

namespace gestionMissionBack.Tests.Services
{
    // Helper class for testing async queryable operations
    public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(
                    name: nameof(IQueryProvider.Execute),
                    genericParameterCount: 1,
                    types: new[] { typeof(Expression) })
                .MakeGenericMethod(resultType)
                .Invoke(this, new[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { executionResult });
        }
    }

    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        public T Current
        {
            get { return _inner.Current; }
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }

    namespace gestionMissionBack.Tests.Services
    {
        public class MissionServiceTests
        {
            private readonly Mock<IMissionRepository> _mockMissionRepository;
            private readonly Mock<ICircuitService> _mockCircuitService;
            private readonly Mock<IRouteService> _mockRouteService;
            private readonly Mock<INotificationService> _mockNotificationService;
            private readonly Mock<IMapper> _mockMapper;
            private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
            private readonly Mock<IConfiguration> _mockConfiguration;
            private readonly Mock<HttpClient> _mockHttpClient;
            private readonly MissionService _missionService;

            public MissionServiceTests()
            {
                _mockMissionRepository = new Mock<IMissionRepository>();
                _mockCircuitService = new Mock<ICircuitService>();
                _mockRouteService = new Mock<IRouteService>();
                _mockNotificationService = new Mock<INotificationService>();
                _mockMapper = new Mock<IMapper>();
                _mockHttpClientFactory = new Mock<IHttpClientFactory>();
                _mockConfiguration = new Mock<IConfiguration>();
                _mockHttpClient = new Mock<HttpClient>();

                _mockHttpClientFactory.Setup(x => x.CreateClient("OpenRouteService")).Returns(_mockHttpClient.Object);
                _mockConfiguration.Setup(x => x["OpenRouteService:ApiKey"]).Returns("test-api-key");

                _missionService = new MissionService(
                    _mockMissionRepository.Object,
                    _mockCircuitService.Object,
                    _mockRouteService.Object,
                    _mockNotificationService.Object,
                    _mockMapper.Object,
                    _mockHttpClientFactory.Object,
                    _mockConfiguration.Object
                );
            }

            [Fact]
            public async Task GetMissionByIdAsync_ShouldReturnMission_WhenMissionExists()
            {
                // Arrange
                var mission = new Mission { MissionId = 1, Service = "Test Mission", Status = MissionStatus.Requested };
                var missionDto = new MissionDto { MissionId = 1, Service = "Test Mission", Status = MissionStatus.Requested };

                _mockMissionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(mission);
                _mockMapper.Setup(x => x.Map<MissionDto>(mission)).Returns(missionDto);

                // Act
                var result = await _missionService.GetMissionByIdAsync(1);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(1, result.MissionId);
                _mockMissionRepository.Verify(x => x.GetByIdAsync(1), Times.Once);
            }

            [Fact]
            public async Task GetMissionByIdAsync_ShouldReturnNull_WhenMissionDoesNotExist()
            {
                // Arrange
                _mockMissionRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Mission)null);
                _mockMapper.Setup(x => x.Map<MissionDto>((Mission)null)).Returns((MissionDto)null);

                // Act
                var result = await _missionService.GetMissionByIdAsync(999);

                // Assert
                Assert.Null(result);
                _mockMissionRepository.Verify(x => x.GetByIdAsync(999), Times.Once);
            }

            //[Fact]
            //public async Task CreateMissionAsync_ShouldCreateMissionAndSendNotification()
            //{
            //    // Arrange
            //    var missionDto = new MissionDto
            //    {
            //        Service = "Test Mission",
            //        Receiver = "Test Receiver",
            //        DriverId = 1,
            //        Status = MissionStatus.Requested
            //    };

            //    var mission = new Mission
            //    {
            //        MissionId = 1,
            //        Service = "Test Mission",
            //        Receiver = "Test Receiver",
            //        DriverId = 1,
            //        Status = MissionStatus.Requested
            //    };

            //    var createdMission = new Mission
            //    {
            //        MissionId = 1,
            //        Service = "Test Mission",
            //        Receiver = "Test Receiver",
            //        DriverId = 1,
            //        Status = MissionStatus.Requested
            //    };

            //    var createdMissionDto = new MissionDto
            //    {
            //        MissionId = 1,
            //        Service = "Test Mission",
            //        Receiver = "Test Receiver",
            //        DriverId = 1,
            //        Status = MissionStatus.Requested
            //    };

            //    _mockMapper.Setup(x => x.Map<Mission>(missionDto)).Returns(mission);
            //    _mockMissionRepository.Setup(x => x.AddAsync(It.IsAny<Mission>())).ReturnsAsync(createdMission);
            //    _mockMapper.Setup(x => x.Map<MissionDto>(createdMission)).Returns(createdMissionDto);
            //    _mockNotificationService.Setup(x => x.SendRealTimeNotificationAsync(It.IsAny<int>(), It.IsAny<CreateNotificationDto>()))
            //        .Returns(Task.CompletedTask);

            //    // Act
            //    var result = await _missionService.CreateMissionAsync(missionDto);

            //    // Assert
            //    Assert.NotNull(result);
            //    Assert.Equal(1, result.MissionId);
            //    _mockMissionRepository.Verify(x => x.AddAsync(It.IsAny<Mission>()), Times.Once);
            //    _mockNotificationService.Verify(x => x.SendRealTimeNotificationAsync(It.IsAny<int>(), It.IsAny<CreateNotificationDto>()), Times.Once);
            //}

            [Fact]
            public async Task UpdateMissionAsync_ShouldUpdateMission_WhenMissionExists()
            {
                // Arrange
                var missionDto = new MissionDto
                {
                    MissionId = 1,
                    Service = "Updated Mission",
                    Status = MissionStatus.InProgress
                };

                var originalMission = new Mission
                {
                    MissionId = 1,
                    Service = "Original Mission",
                    Status = MissionStatus.Requested,
                    DriverId = 1,
                    RequesterId = 2
                };

                _mockMissionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(originalMission);
                _mockMissionRepository.Setup(x => x.UpdateAsync(It.IsAny<Mission>())).ReturnsAsync(true);
                _mockNotificationService.Setup(x => x.SendRealTimeNotificationAsync(It.IsAny<int>(), It.IsAny<CreateNotificationDto>()))
                    .Returns(Task.CompletedTask);

                // Mock the mapper to properly update the mission status
                _mockMapper.Setup(x => x.Map(It.IsAny<MissionDto>(), It.IsAny<Mission>()))
                    .Callback<MissionDto, Mission>((dto, mission) =>
                    {
                        mission.Status = dto.Status;
                        mission.Service = dto.Service;
                    });

                // Act
                var result = await _missionService.UpdateMissionAsync(missionDto);

                // Assert
                Assert.True(result);
                _mockMissionRepository.Verify(x => x.UpdateAsync(It.IsAny<Mission>()), Times.Once);
                // Verify that notifications were sent (status changed from Requested to InProgress)
                _mockNotificationService.Verify(x => x.SendRealTimeNotificationAsync(It.IsAny<int>(), It.IsAny<CreateNotificationDto>()), Times.Exactly(2));
            }

            [Fact]
            public async Task UpdateMissionAsync_ShouldReturnFalse_WhenMissionDoesNotExist()
            {
                // Arrange
                var missionDto = new MissionDto { MissionId = 999 };
                _mockMissionRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Mission)null);

                // Act
                var result = await _missionService.UpdateMissionAsync(missionDto);

                // Assert
                Assert.False(result);
                _mockMissionRepository.Verify(x => x.UpdateAsync(It.IsAny<Mission>()), Times.Never);
                _mockNotificationService.Verify(x => x.SendRealTimeNotificationAsync(It.IsAny<int>(), It.IsAny<CreateNotificationDto>()), Times.Never);
            }

            [Fact]
            public async Task UpdateMissionAsync_ShouldNotSendNotifications_WhenStatusUnchanged()
            {
                // Arrange
                var missionDto = new MissionDto
                {
                    MissionId = 1,
                    Service = "Updated Mission",
                    Status = MissionStatus.Requested // Same status
                };

                var originalMission = new Mission
                {
                    MissionId = 1,
                    Service = "Original Mission",
                    Status = MissionStatus.Requested,
                    DriverId = 1,
                    RequesterId = 2
                };

                _mockMissionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(originalMission);
                _mockMissionRepository.Setup(x => x.UpdateAsync(It.IsAny<Mission>())).ReturnsAsync(true);

                // Act
                var result = await _missionService.UpdateMissionAsync(missionDto);

                // Assert
                Assert.True(result);
                _mockMissionRepository.Verify(x => x.UpdateAsync(It.IsAny<Mission>()), Times.Once);
                _mockNotificationService.Verify(x => x.SendRealTimeNotificationAsync(It.IsAny<int>(), It.IsAny<CreateNotificationDto>()), Times.Never);
            }

            [Fact]
            public async Task DeleteMissionAsync_ShouldReturnTrue_WhenMissionExists()
            {
                // Arrange
                _mockMissionRepository.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

                // Act
                var result = await _missionService.DeleteMissionAsync(1);

                // Assert
                Assert.True(result);
                _mockMissionRepository.Verify(x => x.DeleteAsync(1), Times.Once);
            }

            [Fact]
            public async Task DeleteMissionAsync_ShouldReturnFalse_WhenMissionDoesNotExist()
            {
                // Arrange
                _mockMissionRepository.Setup(x => x.DeleteAsync(999)).ReturnsAsync(false);

                // Act
                var result = await _missionService.DeleteMissionAsync(999);

                // Assert
                Assert.False(result);
                _mockMissionRepository.Verify(x => x.DeleteAsync(999), Times.Once);
            }

            //[Fact]
            //public async Task GetPagedAsync_ShouldReturnPagedMissions_WithoutFilters()
            //{
            //    // Arrange
            //    var missions = new List<Mission>
            //{
            //    new Mission { MissionId = 1, Service = "Mission 1", Status = MissionStatus.Requested },
            //    new Mission { MissionId = 2, Service = "Mission 2", Status = MissionStatus.InProgress }
            //};

            //    var missionDtos = new List<MissionDtoGet>
            //{
            //    new MissionDtoGet { MissionId = 1, Service = "Mission 1", Status = MissionStatus.Requested },
            //    new MissionDtoGet { MissionId = 2, Service = "Mission 2", Status = MissionStatus.InProgress }
            //};

            //    // Create a mock queryable that supports async operations
            //    var mockQueryable = new Mock<IQueryable<Mission>>();
            //    mockQueryable.Setup(x => x.Provider).Returns(new TestAsyncQueryProvider<Mission>(missions.AsQueryable().Provider));
            //    mockQueryable.Setup(x => x.Expression).Returns(missions.AsQueryable().Expression);
            //    mockQueryable.Setup(x => x.ElementType).Returns(missions.AsQueryable().ElementType);
            //    mockQueryable.Setup(x => x.GetEnumerator()).Returns(missions.GetEnumerator());

            //    _mockMissionRepository.Setup(x => x.GetQueryable()).Returns(mockQueryable.Object);
            //    _mockMapper.Setup(x => x.Map<IEnumerable<MissionDtoGet>>(It.IsAny<IEnumerable<Mission>>())).Returns(missionDtos);

            //    // Act
            //    var result = await _missionService.GetPagedAsync(1, 10);

            //    // Assert
            //    Assert.NotNull(result);
            //    Assert.Equal(2, result.TotalRecords);
            //    Assert.Equal(2, result.Data.Count());
            //}

            //[Fact]
            //public async Task GetPagedAsync_ShouldApplyFilters_WhenFiltersProvided()
            //{
            //    // Arrange
            //    var missions = new List<Mission>
            //{
            //    new Mission { MissionId = 1, Service = "Mission 1", Status = MissionStatus.Requested, DriverId = 1 },
            //    new Mission { MissionId = 2, Service = "Mission 2", Status = MissionStatus.InProgress, DriverId = 2 }
            //};

            //    var missionDtos = new List<MissionDtoGet>
            //{
            //    new MissionDtoGet { MissionId = 1, Service = "Mission 1", Status = MissionStatus.Requested }
            //};

            //    var filter = new MissionFilter
            //    {
            //        Status = MissionStatus.Requested,
            //        DriverId = 1
            //    };

            //    // Create a mock queryable that supports async operations
            //    var mockQueryable = new Mock<IQueryable<Mission>>();
            //    mockQueryable.Setup(x => x.Provider).Returns(new TestAsyncQueryProvider<Mission>(missions.AsQueryable().Provider));
            //    mockQueryable.Setup(x => x.Expression).Returns(missions.AsQueryable().Expression);
            //    mockQueryable.Setup(x => x.ElementType).Returns(missions.AsQueryable().ElementType);
            //    mockQueryable.Setup(x => x.GetEnumerator()).Returns(missions.GetEnumerator());

            //    _mockMissionRepository.Setup(x => x.GetQueryable()).Returns(mockQueryable.Object);
            //    _mockMapper.Setup(x => x.Map<IEnumerable<MissionDtoGet>>(It.IsAny<IEnumerable<Mission>>())).Returns(missionDtos);

            //    // Act
            //    var result = await _missionService.GetPagedAsync(1, 10, filter);

            //    // Assert
            //    Assert.NotNull(result);
            //    Assert.Equal(1, result.TotalRecords);
            //}

            [Fact]
            public async Task GenerateCircuitsForMissionAsync_ShouldReturnFalse_WhenMissionDoesNotExist()
            {
                // Arrange
                _mockMissionRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Mission)null);

                // Act
                var result = await _missionService.GenerateCircuitsForMissionAsync(999);

                // Assert
                Assert.False(result);
                _mockCircuitService.Verify(x => x.GetCircuitsByMissionIdAsync(It.IsAny<int>()), Times.Never);
            }

            //[Fact]
            //public async Task GenerateCircuitsForMissionAsync_ShouldReturnFalse_WhenNoTasksExist()
            //{
            //    // Arrange
            //    var mission = new Mission { MissionId = 1, Service = "Test Mission" };
            //    _mockMissionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(mission);

            //    // Create a mission with no tasks
            //    var missions = new List<Mission> { mission };

            //    // Create a mock queryable that supports async operations
            //    var mockQueryable = new Mock<IQueryable<Mission>>();
            //    mockQueryable.Setup(x => x.Provider).Returns(new TestAsyncQueryProvider<Mission>(missions.AsQueryable().Provider));
            //    mockQueryable.Setup(x => x.Expression).Returns(missions.AsQueryable().Expression);
            //    mockQueryable.Setup(x => x.ElementType).Returns(missions.AsQueryable().ElementType);
            //    mockQueryable.Setup(x => x.GetEnumerator()).Returns(missions.GetEnumerator());

            //    _mockMissionRepository.Setup(x => x.GetQueryable()).Returns(mockQueryable.Object);

            //    // Act
            //    var result = await _missionService.GenerateCircuitsForMissionAsync(1);

            //    // Assert
            //    Assert.False(result);
            //    _mockCircuitService.Verify(x => x.GetCircuitsByMissionIdAsync(It.IsAny<int>()), Times.Never);
            //}

            [Fact]
            public void Constructor_ShouldThrowArgumentNullException_WhenApiKeyIsMissing()
            {
                // Arrange
                _mockConfiguration.Setup(x => x["OpenRouteService:ApiKey"]).Returns((string)null);

                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => new MissionService(_mockMissionRepository.Object, _mockCircuitService.Object, _mockRouteService.Object, _mockNotificationService.Object, _mockMapper.Object, _mockHttpClientFactory.Object, _mockConfiguration.Object));
            }
        }
    }

}