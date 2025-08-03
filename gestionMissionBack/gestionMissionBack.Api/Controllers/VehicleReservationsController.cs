using gestionMissionBack.Application.DTOs.VehicleReservation;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Domain.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleReservationsController : ControllerBase
    {
        private readonly IVehicleReservationService _reservationService;

        public VehicleReservationsController(IVehicleReservationService reservationService)
        {
            _reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
        }

        // GET: api/vehiclereservations/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<VehicleReservationDtoGet>>> GetPagedReservations(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] int? requesterId = null,
            [FromQuery] int? vehicleId = null,
            [FromQuery] string status = null,
            [FromQuery] bool? requiresDriver = null,
            [FromQuery] DateTime? startDateFrom = null,
            [FromQuery] DateTime? startDateTo = null,
            [FromQuery] DateTime? endDateFrom = null,
            [FromQuery] DateTime? endDateTo = null,
            [FromQuery] string departure = null,
            [FromQuery] string destination = null)
        {
            var filter = new VehicleReservationFilter
            {
                RequesterId = requesterId,
                VehicleId = vehicleId,
                Status = !string.IsNullOrEmpty(status) ? Enum.Parse<VehicleReservationStatus>(status) : null,
                RequiresDriver = requiresDriver,
                StartDateFrom = startDateFrom,
                StartDateTo = startDateTo,
                EndDateFrom = endDateFrom,
                EndDateTo = endDateTo,
                Departure = departure,
                Destination = destination
            };

            var pagedReservations = await _reservationService.GetPagedAsync(pageNumber, pageSize, filter);
            return Ok(pagedReservations);
        }

        // GET: api/vehiclereservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleReservationDtoGet>>> GetAllReservations()
        {
            var reservations = await _reservationService.GetAllReservationsAsync();
            return Ok(reservations);
        }

        // GET: api/vehiclereservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleReservationDtoGet>> GetReservation(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            return Ok(reservation);
        }

        // GET: api/vehiclereservations/vehicle/5
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<VehicleReservationDtoGet>>> GetReservationsByVehicleId(int vehicleId)
        {
            var reservations = await _reservationService.GetReservationsByVehicleIdAsync(vehicleId);
            return Ok(reservations);
        }

        // GET: api/vehiclereservations/requester/5
        [HttpGet("requester/{requesterId}")]
        public async Task<ActionResult<IEnumerable<VehicleReservationDtoGet>>> GetReservationsByRequesterId(int requesterId)
        {
            var reservations = await _reservationService.GetReservationsByRequesterIdAsync(requesterId);
            return Ok(reservations);
        }

        // GET: api/vehiclereservations/status/Pending
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<VehicleReservationDtoGet>>> GetReservationsByStatus(string status)
        {
            var reservations = await _reservationService.GetReservationsByStatusAsync(status);
            return Ok(reservations);
        }

        // POST: api/vehiclereservations
        [HttpPost]
        public async Task<ActionResult<VehicleReservationDto>> CreateReservation([FromBody] VehicleReservationDto reservationDto)
        {
            var createdReservation = await _reservationService.CreateReservationAsync(reservationDto);
            return CreatedAtAction(nameof(GetReservation), new { id = createdReservation.ReservationId }, createdReservation);
        }

        // PUT: api/vehiclereservations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] VehicleReservationDto reservationDto)
        {
            if (id != reservationDto.ReservationId)
            {
                return BadRequest("Reservation ID in URL must match Reservation ID in body");
            }

            await _reservationService.UpdateReservationAsync(reservationDto);
            return NoContent();
        }

        // PATCH: api/vehiclereservations/5/approve
        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> ApproveReservation(int id)
        {
            var success = await _reservationService.ApproveReservationAsync(id);
            if (!success)
            {
                return NotFound($"Reservation with ID {id} not found");
            }
            return Ok();
        }

        // DELETE: api/vehiclereservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            await _reservationService.DeleteReservationAsync(id);
            return NoContent();
        }
    }
}