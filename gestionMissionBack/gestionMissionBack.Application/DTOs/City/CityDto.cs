using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.DTOs.City
{
    public class CityDto
    {
        public int CityId { get; set; }

        public string Name { get; set; }

        public string PostalCode { get; set; }

        public string Region { get; set; }
    }
}
