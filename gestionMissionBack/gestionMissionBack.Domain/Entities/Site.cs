using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Domain.Entities
{
    public partial class Site
    {
        public int SiteId { get; set; }

        public string Name { get; set; }

        public string Type { get; set; } // Depot, Fournisseur, Establishment

        public string Address { get; set; }

        public string? Phone { get; set; }

        public int CityId { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public virtual City City { get; set; }

        public virtual ICollection<TaskMission> Tasks { get; set; } = new List<TaskMission>();
    }

}
