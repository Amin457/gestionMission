using gestionMissionBack.Domain.Enums;
using System;

namespace gestionMissionBack.Domain.Helpers
{
    public class VehicleReservationFilter
    {
        public int? RequesterId { get; set; }
        public int? VehicleId { get; set; }
        public VehicleReservationStatus? Status { get; set; }
        public bool? RequiresDriver { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
    }
} 