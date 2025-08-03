using gestionMissionBack.Application.DTOs.Route;
using gestionMissionBack.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService _routeService;

        public RoutesController(IRouteService routeService)
        {
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
        }

        // GET: api/routes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteDto>>> GetAllRoutes()
        {
            var routes = await _routeService.GetAllRoutesAsync();
            return Ok(routes);
        }

        // GET: api/routes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RouteDto>> GetRoute(int id)
        {
            var route = await _routeService.GetRouteByIdAsync(id);
            return Ok(route);
        }

        // GET: api/routes/circuit/5
        [HttpGet("circuit/{circuitId}")]
        public async Task<ActionResult<IEnumerable<RouteDto>>> GetRoutesByCircuitId(int circuitId)
        {
            var routes = await _routeService.GetRoutesByCircuitIdAsync(circuitId);
            return Ok(routes);
        }

        // GET: api/routes/totaldistance/circuit/5
        [HttpGet("totaldistance/circuit/{circuitId}")]
        public async Task<ActionResult<double>> GetTotalDistanceByCircuitId(int circuitId)
        {
            var totalDistance = await _routeService.GetTotalDistanceByCircuitIdAsync(circuitId);
            return Ok(totalDistance);
        }

        // POST: api/routes
        [HttpPost]
        public async Task<ActionResult<RouteDto>> CreateRoute([FromBody] RouteDto routeDto)
        {
            var createdRoute = await _routeService.CreateRouteAsync(routeDto);
            return CreatedAtAction(nameof(GetRoute), new { id = createdRoute.RouteId }, createdRoute);
        }

        // PUT: api/routes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] RouteDto routeDto)
        {
            if (id != routeDto.RouteId)
            {
                throw new ArgumentException("Route ID in URL must match Route ID in body");
            }

            await _routeService.UpdateRouteAsync(routeDto);
            return NoContent();
        }

        // DELETE: api/routes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            await _routeService.DeleteRouteAsync(id);
            return NoContent();
        }
    }
}