using System;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.DTOs.VehicleReservation
{
    public class VehicleReservationDtoGet
    {
        public int ReservationId { get; set; }
        public int RequesterId { get; set; }
        public string RequesterName { get; set; }
        public int VehicleId { get; set; }
        public string VehicleLicensePlate { get; set; }
        public bool RequiresDriver { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public VehicleReservationStatus Status { get; set; }
    }
} 