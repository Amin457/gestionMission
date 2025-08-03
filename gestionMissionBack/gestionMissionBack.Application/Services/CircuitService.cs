using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.DTOs.Circuit;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Validators;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Services
{
    public class CircuitService : ICircuitService
    {
        private readonly ICircuitRepository _circuitRepository;
        private readonly IValidator<CircuitDto> _circuitValidator;
        private readonly IMapper _mapper;

        public CircuitService(
            ICircuitRepository circuitRepository,
            IValidator<CircuitDto> circuitValidator,
            IMapper mapper)
        {
            _circuitRepository = circuitRepository ?? throw new ArgumentNullException(nameof(circuitRepository));
            _circuitValidator = circuitValidator ?? throw new ArgumentNullException(nameof(circuitValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<CircuitDto> GetCircuitByIdAsync(int id)
        {
            var circuit = await _circuitRepository.GetByIdAsync(id);
            return _mapper.Map<CircuitDto>(circuit);
        }

        public async Task<IEnumerable<CircuitDto>> GetAllCircuitsAsync()
        {
            var circuits = await _circuitRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CircuitDto>>(circuits);
        }

        public async Task<IEnumerable<CircuitDto>> GetCircuitsByMissionIdAsync(int missionId)
        {
            var circuits = await _circuitRepository.GetCircuitsByMissionIdAsync(missionId);
            return _mapper.Map<IEnumerable<CircuitDto>>(circuits);
        }

        public async Task<CircuitDto> GetCircuitByMissionIdAsync(int missionId)
        {
            var circuit = await _circuitRepository.GetCircuitByMissionIdAsync(missionId);
            return _mapper.Map<CircuitDto>(circuit);
        }

        public async Task<CircuitDto> CreateCircuitAsync(CircuitDto circuitDto)
        {
            var validationResult = await _circuitValidator.ValidateAsync(circuitDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var circuit = _mapper.Map<Circuit>(circuitDto);
            var createdCircuit = await _circuitRepository.AddAsync(circuit);
            return _mapper.Map<CircuitDto>(createdCircuit);
        }

        public async Task UpdateCircuitAsync(CircuitDto circuitDto)
        {
            var validationResult = await _circuitValidator.ValidateAsync(circuitDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingCircuit = await _circuitRepository.GetByIdAsync(circuitDto.CircuitId);
            _mapper.Map(circuitDto, existingCircuit);
            var updated = await _circuitRepository.UpdateAsync(existingCircuit);
            if (!updated)
            {
                throw new Exception($"Failed to update circuit with ID {circuitDto.CircuitId}");
            }
        }

        public async Task DeleteCircuitAsync(int id)
        {
            var deleted = await _circuitRepository.DeleteAsync(id);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Circuit with ID {id} not found or could not be deleted");
            }
        }

        public async Task<int> GetCircuitCountByMissionIdAsync(int missionId)
        {
            return await _circuitRepository.GetCircuitCountByMissionIdAsync(missionId);
        }
    }
}