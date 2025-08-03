using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.DTOs.Route;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Validators;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Services
{
    public class RouteService : IRouteService
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IValidator<RouteDto> _routeValidator;
        private readonly IMapper _mapper;

        public RouteService(
            IRouteRepository routeRepository,
            IValidator<RouteDto> routeValidator,
            IMapper mapper)
        {
            _routeRepository = routeRepository ?? throw new ArgumentNullException(nameof(routeRepository));
            _routeValidator = routeValidator ?? throw new ArgumentNullException(nameof(routeValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<RouteDto> GetRouteByIdAsync(int id)
        {
            var route = await _routeRepository.GetByIdAsync(id);
            return _mapper.Map<RouteDto>(route);
        }

        public async Task<IEnumerable<RouteDto>> GetAllRoutesAsync()
        {
            var routes = await _routeRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RouteDto>>(routes);
        }

        public async Task<IEnumerable<RouteDto>> GetRoutesByCircuitIdAsync(int circuitId)
        {
            var routes = await _routeRepository.GetRoutesByCircuitIdAsync(circuitId);
            return _mapper.Map<IEnumerable<RouteDto>>(routes);
        }

        public async Task<double> GetTotalDistanceByCircuitIdAsync(int circuitId)
        {
            return await _routeRepository.GetTotalDistanceByCircuitIdAsync(circuitId);
        }

        public async Task<RouteDto> CreateRouteAsync(RouteDto routeDto)
        {
            var validationResult = await _routeValidator.ValidateAsync(routeDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var route = _mapper.Map<Route>(routeDto);
            var createdRoute = await _routeRepository.AddAsync(route);
            return _mapper.Map<RouteDto>(createdRoute);
        }

        public async Task UpdateRouteAsync(RouteDto routeDto)
        {
            var validationResult = await _routeValidator.ValidateAsync(routeDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingRoute = await _routeRepository.GetByIdAsync(routeDto.RouteId);
            _mapper.Map(routeDto, existingRoute);
            var updated = await _routeRepository.UpdateAsync(existingRoute);
            if (!updated)
            {
                throw new Exception($"Failed to update route with ID {routeDto.RouteId}");
            }
        }

        public async Task DeleteRouteAsync(int id)
        {
            var deleted = await _routeRepository.DeleteAsync(id);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Route with ID {id} not found or could not be deleted");
            }
        }

        public async Task DeleteRoutesByCircuitIdAsync(int circuitId)
        {
            var routes = await _routeRepository.GetRoutesByCircuitIdAsync(circuitId);
            foreach (var route in routes)
            {
                await _routeRepository.DeleteAsync(route.RouteId);
            }
        }
    }
}