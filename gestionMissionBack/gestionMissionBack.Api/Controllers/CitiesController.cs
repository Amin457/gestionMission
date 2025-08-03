using gestionMissionBack.Application.DTOs.City;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController : ControllerBase
    {
        private readonly ICityService _cityService;

        public CitiesController(ICityService cityService)
        {
            _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
        }
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagedCities = await _cityService.GetPagedAsync(pageNumber, pageSize);
            return Ok(pagedCities);
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetAllCities()
        {
            var cities = await _cityService.GetAllCitiesAsync();
            return Ok(cities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CityDto>> GetCity(int id)
        {
            var city = await _cityService.GetCityByIdAsync(id);
            return Ok(city);
        }

        [HttpGet("{id}/sites")]
        public async Task<ActionResult<CityDto>> GetCityWithSites(int id)
        {
            var city = await _cityService.GetCityWithSitesAsync(id);
            return Ok(city);
        }

        [HttpGet("region/{region}")]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetCitiesByRegion(string region)
        {
            var cities = await _cityService.GetCitiesWithRegionAsync(region);
            return Ok(cities);
        }

        [HttpPost]
        public async Task<ActionResult<CityDto>> CreateCity([FromBody] CityDto cityDto)
        {
            var created = await _cityService.CreateCityAsync(cityDto);
            return CreatedAtAction(nameof(GetCity), new { id = created.CityId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCity(int id, [FromBody] CityDto cityDto)
        {
            if (id != cityDto.CityId)
                throw new ArgumentException("Route ID in URL must match City ID in body");

            await _cityService.UpdateCityAsync(cityDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            await _cityService.DeleteCityAsync(id);
            return NoContent();
        }
    }
}
