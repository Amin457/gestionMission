using gestionMissionBack.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.DTOs.Site
{
    public class SiteDto
    {
        public int SiteId { get; set; }

        public string Name { get; set; }

        public string Type { get; set; } // Depot, Fournisseur, Establishment

        public string Address { get; set; }

        public string Phone { get; set; }

        public int CityId { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }
    }
}
