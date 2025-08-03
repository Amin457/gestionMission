using Microsoft.AspNetCore.Http;
using gestionMissionBack.Domain.Enums;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Vehicle
{
    public class VehicleUpdateDto
    {
        public int VehicleId { get; set; }
        public VehicleType Type { get; set; }
        public string LicensePlate { get; set; }
        public bool Availability { get; set; }
        public double MaxCapacity { get; set; }
        public List<string> KeepPhotosUrls { get; set; } = new List<string>(); // Existing photo URLs to keep
        public List<IFormFile> NewPhotos { get; set; } = new List<IFormFile>(); // New photos to upload
    }
} 