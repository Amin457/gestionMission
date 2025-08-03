using gestionMissionBack.Application.DTOs.MissionCost;
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
    public class MissionCostsController : ControllerBase
    {
        private readonly IMissionCostService _missionCostService;

        public MissionCostsController(IMissionCostService missionCostService)
        {
            _missionCostService = missionCostService ?? throw new ArgumentNullException(nameof(missionCostService));
        }

        // GET: api/missioncosts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MissionCostGetDto>>> GetAllCosts()
        {
            var costs = await _missionCostService.GetAllCostsAsync();
            return Ok(costs);
        }

        // GET: api/missioncosts/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<MissionCostGetDto>>> GetPagedCosts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? missionId = null,
            [FromQuery] MissionCostType? type = null,
            [FromQuery] double? minAmount = null,
            [FromQuery] double? maxAmount = null,
            [FromQuery] DateTime? dateStart = null,
            [FromQuery] DateTime? dateEnd = null)
        {
            var filter = new MissionCostFilter
            {
                MissionId = missionId,
                Type = type,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                DateStart = dateStart,
                DateEnd = dateEnd
            };

            var result = await _missionCostService.GetPagedAsync(pageNumber, pageSize, filter);
            return Ok(result);
        }

        // GET: api/missioncosts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MissionCostGetDto>> GetCost(int id)
        {
            var cost = await _missionCostService.GetCostByIdAsync(id);
            return Ok(cost);
        }

        // GET: api/missioncosts/mission/5
        [HttpGet("mission/{missionId}")]
        public async Task<ActionResult<IEnumerable<MissionCostGetDto>>> GetCostsByMissionId(int missionId)
        {
            var costs = await _missionCostService.GetCostsByMissionIdAsync(missionId);
            return Ok(costs);
        }

        // GET: api/missioncosts/total/mission/5
        [HttpGet("total/mission/{missionId}")]
        public async Task<ActionResult<double>> GetTotalCostByMissionId(int missionId)
        {
            var totalCost = await _missionCostService.GetTotalCostByMissionIdAsync(missionId);
            return Ok(totalCost);
        }

        // POST: api/missioncosts
        [HttpPost]
        public async Task<ActionResult<MissionCostGetDto>> CreateCost([FromForm] MissionCostCreateDto costDto)
        {
            var createdCost = await _missionCostService.CreateCostAsync(costDto);
            return CreatedAtAction(nameof(GetCost), new { id = createdCost.CostId }, createdCost);
        }

        // PUT: api/missioncosts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCost(int id, [FromForm] MissionCostUpdateDto costDto)
        {
            await _missionCostService.UpdateCostAsync(id, costDto);
            return NoContent();
        }

        // DELETE: api/missioncosts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCost(int id)
        {
            await _missionCostService.DeleteCostAsync(id);
            return NoContent();
        }
    }
}