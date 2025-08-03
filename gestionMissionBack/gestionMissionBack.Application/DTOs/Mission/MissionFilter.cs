using gestionMissionBack.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.DTOs.Mission
{
    public class MissionFilter
    {
        public MissionType? Type { get; set; }
        public MissionStatus? Status { get; set; }
        public int? DriverId { get; set; }
        public int? Quantity { get; set; }
        public DateTime? DesiredDateStart { get; set; }
        public DateTime? DesiredDateEnd { get; set; }
    }
}
