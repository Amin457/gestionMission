using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.DTOs;
using gestionMissionBack.Application.DTOs.Site;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Services
{
    public class SiteService : ISiteService
    {
        private readonly ISiteRepository _siteRepository;
        private readonly IValidator<SiteDto> _siteValidator;
        private readonly IMapper _mapper;

        public SiteService(ISiteRepository siteRepository, IValidator<SiteDto> siteValidator, IMapper mapper)
        {
            _siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
            _siteValidator = siteValidator ?? throw new ArgumentNullException(nameof(siteValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<SiteDtoGet> GetSiteByIdAsync(int id)
        {
            var query = _siteRepository.GetQueryable();
            var site = await query
                .Include(s => s.City)
                .FirstOrDefaultAsync(s => s.SiteId == id);
            return _mapper.Map<SiteDtoGet>(site);
        }

        public async Task<PagedResult<SiteDtoGet>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _siteRepository.GetQueryable();
            var totalRecords = await query.CountAsync();
            var data = await query
                .Include(s => s.City)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<SiteDtoGet>
            {
                Data = _mapper.Map<IEnumerable<SiteDtoGet>>(data),
                TotalRecords = totalRecords
            };
        }

        public async Task<IEnumerable<SiteDtoGet>> GetAllSitesAsync()
        {
            var query = _siteRepository.GetQueryable();
            var sites = await query
                .Include(s => s.City)
                .ToListAsync();
            return _mapper.Map<IEnumerable<SiteDtoGet>>(sites);
        }

        public async Task<IEnumerable<SiteDtoGet>> GetSitesByCityIdAsync(int cityId)
        {
            var query = _siteRepository.GetQueryable();
            var sites = await query
                .Include(s => s.City)
                .Where(s => s.CityId == cityId)
                .ToListAsync();
            return _mapper.Map<IEnumerable<SiteDtoGet>>(sites);
        }

        public async Task<IEnumerable<SiteDtoGet>> GetSitesByTypeAsync(string type)
        {
            var query = _siteRepository.GetQueryable();
            var sites = await query
                .Include(s => s.City)
                .Where(s => s.Type.ToLower() == type.ToLower())
                .ToListAsync();
            return _mapper.Map<IEnumerable<SiteDtoGet>>(sites);
        }

        public async Task<SiteDto> CreateSiteAsync(SiteDto siteDto)
        {
            var validationResult = await _siteValidator.ValidateAsync(siteDto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var site = _mapper.Map<Site>(siteDto);
            var created = await _siteRepository.AddAsync(site);
            return _mapper.Map<SiteDto>(created);
        }

        public async Task UpdateSiteAsync(SiteDto siteDto)
        {
            var validationResult = await _siteValidator.ValidateAsync(siteDto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var existing = await _siteRepository.GetByIdAsync(siteDto.SiteId);
            _mapper.Map(siteDto, existing);
            var updated = await _siteRepository.UpdateAsync(existing);
            if (!updated)
                throw new Exception($"Failed to update site with ID {siteDto.SiteId}");
        }

        public async Task DeleteSiteAsync(int id)
        {
            var deleted = await _siteRepository.DeleteAsync(id);
            if (!deleted)
                throw new KeyNotFoundException($"Site with ID {id} not found or could not be deleted");
        }
    }
}
