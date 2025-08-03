using gestionMissionBack.Domain.Enums;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Vehicle
{
    public class VehicleDto
    {
        public int VehicleId { get; set; }
        public VehicleType Type { get; set; }
        public string LicensePlate { get; set; }
        public bool Availability { get; set; }
        public double MaxCapacity { get; set; }
        public List<string> PhotoUrls { get; set; } = new List<string>();
    }
}