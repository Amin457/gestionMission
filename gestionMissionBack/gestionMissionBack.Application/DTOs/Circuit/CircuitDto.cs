using gestionMissionBack.Application.DTOs.Route;
using System;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Circuit
{
    public class CircuitDto
    {
        public int CircuitId { get; set; }
        public int MissionId { get; set; }
        public DateTime DepartureDate { get; set; }
        public int DepartureSiteId { get; set; }
        public int ArrivalSiteId { get; set; }
        public string DepartureSiteName { get; set; }
        public string ArrivalSiteName { get; set; }
        public virtual ICollection<RouteDto> Routes { get; set; } = new List<RouteDto>();
    }
}