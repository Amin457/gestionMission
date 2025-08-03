using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.DTOs.Route
{
    public class RouteDto
    {
        public int RouteId { get; set; }
        public int CircuitId { get; set; }
        public int DepartureSiteId { get; set; }
        public int ArrivalSiteId { get; set; }
        public double? DistanceKm { get; set; }
        public string DepartureSiteName { get; set; }
        public string ArrivalSiteName { get; set; }
        public int Ordre { get; set; }
    }
}