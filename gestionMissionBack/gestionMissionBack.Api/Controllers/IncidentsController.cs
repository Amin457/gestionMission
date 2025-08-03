using gestionMissionBack.Application.DTOs.Incident;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Domain.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentsController : ControllerBase
    {
        private readonly IIncidentService _incidentService;

        public IncidentsController(IIncidentService incidentService)
        {
            _incidentService = incidentService ?? throw new ArgumentNullException(nameof(incidentService));
        }

        // GET: api/incidents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncidentGetDto>>> GetAllIncidents()
        {
            var incidents = await _incidentService.GetAllIncidentsAsync();
            return Ok(incidents);
        }

        // GET: api/incidents/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<IncidentGetDto>>> GetPagedIncidents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? missionId = null,
            [FromQuery] IncidentType? type = null,
            [FromQuery] IncidentStatus? status = null,
            [FromQuery] DateTime? reportDateStart = null,
            [FromQuery] DateTime? reportDateEnd = null)
        {
            var filter = new IncidentFilter
            {
                MissionId = missionId,
                Type = type,
                Status = status,
                ReportDateStart = reportDateStart,
                ReportDateEnd = reportDateEnd
            };

            var result = await _incidentService.GetPagedAsync(pageNumber, pageSize, filter);
            return Ok(result);
        }

        // GET: api/incidents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IncidentGetDto>> GetIncident(int id)
        {
            var incident = await _incidentService.GetIncidentByIdAsync(id);
            return Ok(incident);
        }

        // GET: api/incidents/mission/5
        [HttpGet("mission/{missionId}")]
        public async Task<ActionResult<IEnumerable<IncidentGetDto>>> GetIncidentsByMissionId(int missionId)
        {
            var incidents = await _incidentService.GetIncidentsByMissionIdAsync(missionId);
            return Ok(incidents);
        }

        // GET: api/incidents/count/mission/5
        [HttpGet("count/mission/{missionId}")]
        public async Task<ActionResult<int>> GetIncidentCountByMissionId(int missionId)
        {
            var count = await _incidentService.GetIncidentCountByMissionIdAsync(missionId);
            return Ok(count);
        }

        // POST: api/incidents
        [HttpPost]
        public async Task<ActionResult<IncidentGetDto>> CreateIncident([FromForm] IncidentCreateDto incidentDto)
        {
            var createdIncident = await _incidentService.CreateIncidentAsync(incidentDto);
            return CreatedAtAction(nameof(GetIncident), new { id = createdIncident.IncidentId }, createdIncident);
        }

        // PUT: api/incidents/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIncident(int id, [FromForm] IncidentUpdateDto incidentDto)
        {
            await _incidentService.UpdateIncidentAsync(id, incidentDto);
            return NoContent();
        }

        // DELETE: api/incidents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            await _incidentService.DeleteIncidentAsync(id);
            return NoContent();
        }
    }
}