using Microsoft.AspNetCore.Http;
using gestionMissionBack.Domain.Enums;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Vehicle
{
    public class VehicleCreateDto
    {
        public VehicleType Type { get; set; }
        public string LicensePlate { get; set; }
        public bool Availability { get; set; }
        public double MaxCapacity { get; set; }
        public List<IFormFile> Photos { get; set; } = new List<IFormFile>(); // Photos to upload
    }
} 