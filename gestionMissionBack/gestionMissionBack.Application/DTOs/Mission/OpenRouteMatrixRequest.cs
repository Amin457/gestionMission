using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Mission
{
    public class OpenRouteMatrixRequest
    {
        public List<List<double>> Locations { get; set; }
        public List<string> Metrics { get; set; } = new List<string> { "distance", "duration" };
        public string Units { get; set; } = "m";
        public string Profile { get; set; } = "driving-car";
        public List<string> Sources { get; set; } = new List<string> { "all" };
        public List<string> Destinations { get; set; } = new List<string> { "all" };
    }
} 