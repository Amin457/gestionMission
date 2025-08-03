using gestionMissionBack.Application.DTOs.Circuit;
using gestionMissionBack.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CircuitsController : ControllerBase
    {
        private readonly ICircuitService _circuitService;

        public CircuitsController(ICircuitService circuitService)
        {
            _circuitService = circuitService ?? throw new ArgumentNullException(nameof(circuitService));
        }

        // GET: api/circuits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CircuitDto>>> GetAllCircuits()
        {
            var circuits = await _circuitService.GetAllCircuitsAsync();
            return Ok(circuits);
        }

        // GET: api/circuits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CircuitDto>> GetCircuit(int id)
        {
            var circuit = await _circuitService.GetCircuitByIdAsync(id);
            return Ok(circuit);
        }

        // GET: api/circuits/mission/5
        [HttpGet("mission/{missionId}")]
        public async Task<ActionResult<IEnumerable<CircuitDto>>> GetCircuitsByMissionId(int missionId)
        {
            var circuits = await _circuitService.GetCircuitsByMissionIdAsync(missionId);
            return Ok(circuits);
        }

        // GET: api/circuits/count/mission/5
        [HttpGet("count/mission/{missionId}")]
        public async Task<ActionResult<int>> GetCircuitCountByMissionId(int missionId)
        {
            var count = await _circuitService.GetCircuitCountByMissionIdAsync(missionId);
            return Ok(count);
        }

        // POST: api/circuits
        [HttpPost]
        public async Task<ActionResult<CircuitDto>> CreateCircuit([FromBody] CircuitDto circuitDto)
        {
            var createdCircuit = await _circuitService.CreateCircuitAsync(circuitDto);
            return CreatedAtAction(nameof(GetCircuit), new { id = createdCircuit.CircuitId }, createdCircuit);
        }

        // PUT: api/circuits/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCircuit(int id, [FromBody] CircuitDto circuitDto)
        {
            if (id != circuitDto.CircuitId)
            {
                throw new ArgumentException("Route ID in URL must match Route ID in body");
            }

            await _circuitService.UpdateCircuitAsync(circuitDto);
            return NoContent();
        }

        // DELETE: api/circuits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCircuit(int id)
        {
            await _circuitService.DeleteCircuitAsync(id);
            return NoContent();
        }
    }
}