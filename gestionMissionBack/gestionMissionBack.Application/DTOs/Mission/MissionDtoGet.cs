using gestionMissionBack.Application.DTOs.TaskMission;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.DTOs.Mission
{
    public class MissionDtoGet
    {
        public int MissionId { get; set; }
        public MissionType Type { get; set; }
        public int RequesterId { get; set; }
        public string Requester { get; set; }
        public int DriverId { get; set; }
        public string Driver { get; set; }
        public DateTime? SystemDate { get; set; }
        public DateTime? DesiredDate { get; set; }
        public string Service { get; set; }
        public int Quantity { get; set; }
        public string Receiver { get; set; }
        public MissionStatus Status { get; set; }
        public virtual ICollection<TaskMissionDtoGet> Tasks { get; set; } = new List<TaskMissionDtoGet>();

    }
}
