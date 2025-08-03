using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.DTOs.City;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Services
{
    public class CityService : ICityService
    {
        private readonly ICityRepository _cityRepository;
        private readonly IValidator<CityDto> _cityValidator;
        private readonly IMapper _mapper;

        public CityService(ICityRepository cityRepository, IValidator<CityDto> cityValidator, IMapper mapper)
        {
            _cityRepository = cityRepository ?? throw new ArgumentNullException(nameof(cityRepository));
            _cityValidator = cityValidator ?? throw new ArgumentNullException(nameof(cityValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<CityDto> GetCityByIdAsync(int id)
        {
            var city = await _cityRepository.GetByIdAsync(id);
            return _mapper.Map<CityDto>(city);
        }

        public async Task<IEnumerable<CityDto>> GetAllCitiesAsync()
        {
            var cities = await _cityRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CityDto>>(cities);
        }
        public async Task<PagedResult<CityDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var Cities = await _cityRepository.GetPagedAsync(pageNumber, pageSize);

            return new PagedResult<CityDto>
            {
                Data = _mapper.Map<IEnumerable<CityDto>>(Cities.Data),
                TotalRecords = Cities.TotalRecords
            };
        }
        public async Task<IEnumerable<CityDto>> GetCitiesWithRegionAsync(string region)
        {
            var cities = await _cityRepository.GetCitiesWithRegionAsync(region);
            return _mapper.Map<IEnumerable<CityDto>>(cities);
        }

        public async Task<CityDto> GetCityWithSitesAsync(int cityId)
        {
            var city = await _cityRepository.GetCityWithSitesAsync(cityId);
            return _mapper.Map<CityDto>(city);
        }

        public async Task<CityDto> CreateCityAsync(CityDto cityDto)
        {
            var validationResult = await _cityValidator.ValidateAsync(cityDto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            // Check if City already exists
            var existingCity = await _cityRepository.FindByNameAsync(cityDto.Name);
            if (existingCity != null)
            {
                throw new InvalidOperationException("A city with this name already exists.");
            }
            var city = _mapper.Map<City>(cityDto);
            var created = await _cityRepository.AddAsync(city);
            return _mapper.Map<CityDto>(created);
        }

        public async Task UpdateCityAsync(CityDto cityDto)
        {
            var validationResult = await _cityValidator.ValidateAsync(cityDto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var existing = await _cityRepository.GetByIdAsync(cityDto.CityId);
            _mapper.Map(cityDto, existing);
            var updated = await _cityRepository.UpdateAsync(existing);
            if (!updated)
                throw new Exception($"Failed to update city with ID {cityDto.CityId}");
        }

        public async Task DeleteCityAsync(int id)
        {
            var deleted = await _cityRepository.DeleteAsync(id);
            if (!deleted)
                throw new KeyNotFoundException($"City with ID {id} not found or could not be deleted");
        }
    }
}
