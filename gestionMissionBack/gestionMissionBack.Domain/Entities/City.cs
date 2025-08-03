using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Domain.Entities
{
    public partial class City
    {
        public int CityId { get; set; }

        public string Name { get; set; }

        public string PostalCode { get; set; }

        public string Region { get; set; }

        public virtual ICollection<Site> Sites { get; set; } = new List<Site>();
    }

}
