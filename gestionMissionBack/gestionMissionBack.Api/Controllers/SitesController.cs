using gestionMissionBack.Application.DTOs;
using gestionMissionBack.Application.DTOs.Site;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SitesController : ControllerBase
    {
        private readonly ISiteService _siteService;

        public SitesController(ISiteService siteService)
        {
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagedSites = await _siteService.GetPagedAsync(pageNumber, pageSize);
            return Ok(pagedSites);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SiteDtoGet>>> GetAllSites()
        {
            var sites = await _siteService.GetAllSitesAsync();
            return Ok(sites);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SiteDtoGet>> GetSite(int id)
        {
            var site = await _siteService.GetSiteByIdAsync(id);
            return Ok(site);
        }

        [HttpGet("city/{cityId}")]
        public async Task<ActionResult<IEnumerable<SiteDtoGet>>> GetSitesByCityId(int cityId)
        {
            var sites = await _siteService.GetSitesByCityIdAsync(cityId);
            return Ok(sites);
        }

        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<SiteDtoGet>>> GetSitesByType(string type)
        {
            var sites = await _siteService.GetSitesByTypeAsync(type);
            return Ok(sites);
        }

        [HttpPost]
        public async Task<ActionResult<SiteDto>> CreateSite([FromBody] SiteDto siteDto)
        {
            var created = await _siteService.CreateSiteAsync(siteDto);
            return CreatedAtAction(nameof(GetSite), new { id = created.SiteId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSite(int id, [FromBody] SiteDto siteDto)
        {
            if (id != siteDto.SiteId)
                throw new ArgumentException("Route ID in URL must match Site ID in body");

            await _siteService.UpdateSiteAsync(siteDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSite(int id)
        {
            await _siteService.DeleteSiteAsync(id);
            return NoContent();
        }
    }
}
