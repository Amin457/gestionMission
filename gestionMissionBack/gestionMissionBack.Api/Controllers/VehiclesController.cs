using gestionMissionBack.Application.DTOs.Vehicle;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Services;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        }

        // GET: api/vehicles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleGetDto>>> GetAllVehicles()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicles);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] VehicleFilter filter = null)
        {
            var pagedVehicle = await _vehicleService.GetPagedAsync(pageNumber, pageSize, filter);
            return Ok(pagedVehicle);
        }

        // GET: api/vehicles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleGetDto>> GetVehicle(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            return Ok(vehicle);
        }

        // GET: api/vehicles/available
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<VehicleGetDto>>> GetAvailableVehicles()
        {
            var vehicles = await _vehicleService.GetAvailableVehiclesAsync();
            return Ok(vehicles);
        }

        // GET: api/vehicles/count/type/Truck
        [HttpGet("count/type/{type}")]
        public async Task<ActionResult<int>> GetVehicleCountByType(VehicleType type)
        {
            var count = await _vehicleService.GetVehicleCountByTypeAsync(type);
            return Ok(count);
        }

        // POST: api/vehicles
        [HttpPost]
        public async Task<ActionResult<VehicleGetDto>> CreateVehicle([FromForm] VehicleCreateDto vehicleCreateDto)
        {
            var createdVehicle = await _vehicleService.CreateVehicleAsync(vehicleCreateDto);
            return CreatedAtAction(nameof(GetVehicle), new { id = createdVehicle.VehicleId }, createdVehicle);
        }

        // POST: api/vehicles/json (for vehicles without photos - JSON)
        [HttpPost("json")]
        public async Task<ActionResult<VehicleGetDto>> CreateVehicleJson([FromBody] VehicleCreateDto vehicleCreateDto)
        {
            // Set Photos to null for JSON requests
            vehicleCreateDto.Photos = null;
            var createdVehicle = await _vehicleService.CreateVehicleAsync(vehicleCreateDto);
            return CreatedAtAction(nameof(GetVehicle), new { id = createdVehicle.VehicleId }, createdVehicle);
        }

        // PUT: api/vehicles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromForm] VehicleUpdateDto vehicleUpdateDto)
        {
            if (id != vehicleUpdateDto.VehicleId)
            {
                throw new ArgumentException("Route ID in URL must match Vehicle ID in body");
            }

            await _vehicleService.UpdateVehicleAsync(vehicleUpdateDto);
            return NoContent();
        }

        // PUT: api/vehicles/5/json (for vehicles without photos - JSON)
        [HttpPut("{id}/json")]
        public async Task<IActionResult> UpdateVehicleJson(int id, [FromBody] VehicleUpdateDto vehicleUpdateDto)
        {
            if (id != vehicleUpdateDto.VehicleId)
            {
                throw new ArgumentException("Route ID in URL must match Vehicle ID in body");
            }

            // Set NewPhotos to null for JSON requests
            vehicleUpdateDto.NewPhotos = null;
            await _vehicleService.UpdateVehicleAsync(vehicleUpdateDto);
            return NoContent();
        }

        // DELETE: api/vehicles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            await _vehicleService.DeleteVehicleAsync(id);
            return NoContent();
        }
    }
}