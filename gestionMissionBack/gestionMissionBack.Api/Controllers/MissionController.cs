using gestionMissionBack.Application.DTOs.Mission;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Services;
using gestionMissionBack.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/missions")]
    [ApiController]
    public class MissionController : ControllerBase
    {
        private readonly IMissionService _missionService;

        public MissionController(IMissionService missionService)
        {
            _missionService = missionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMissions()
        {
            var missions = await _missionService.GetPagedAsync(1, 1000); // Get all missions
            return Ok(missions);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
                                                        [FromQuery] MissionType? type = null, [FromQuery] MissionStatus? status = null,
                                                        [FromQuery] int? driverId = null,
                                                        [FromQuery] DateTime? desiredDateStart = null, 
                                                        [FromQuery] DateTime? desiredDateEnd = null)
        {
            var filters = new MissionFilter
            {
                Type = type,
                Status = status,
                DriverId = driverId,
                DesiredDateStart = desiredDateStart,
                DesiredDateEnd = desiredDateEnd
            };
            var pagedMissions = await _missionService.GetPagedAsync(pageNumber, pageSize, filters);
            return Ok(pagedMissions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MissionDto>> GetMissionById(int id)
        {
            var mission = await _missionService.GetMissionByIdAsync(id);
            if (mission == null) return NotFound();
            return Ok(mission);
        }

        [HttpPost]
        public async Task<ActionResult<MissionDto>> CreateMission(MissionDto missionDto)
        {
            var newMission = await _missionService.CreateMissionAsync(missionDto);
            return CreatedAtAction(nameof(GetMissionById), new { id = newMission.MissionId }, newMission);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMission(int id, MissionDto missionDto)
        {
            if (id != missionDto.MissionId) throw new ArgumentException("Route ID in URL must match Route ID in body");
            var updated = await _missionService.UpdateMissionAsync(missionDto);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMission(int id)
        {
            var deleted = await _missionService.DeleteMissionAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/generate-circuits")]
        public async Task<IActionResult> GenerateCircuits(int id)
        {
            var success = await _missionService.GenerateCircuitsForMissionAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Circuits generated successfully" });
        }
    }
}
