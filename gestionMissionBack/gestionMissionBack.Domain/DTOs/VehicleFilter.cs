using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Domain.DTOs
{
    public class VehicleFilter
    {
        public VehicleType? Type { get; set; }
        public bool? Availability { get; set; }
        public string? LicensePlate { get; set; }
        public double? MinCapacity { get; set; }
        public double? MaxCapacity { get; set; }
    }
} 