using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Mission
{
    public class OpenRouteMatrixResponse
    {
        public List<List<double>> Distances { get; set; }
        public List<List<double>> Durations { get; set; }
    }
} 