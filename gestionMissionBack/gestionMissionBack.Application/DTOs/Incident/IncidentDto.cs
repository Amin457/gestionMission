using gestionMissionBack.Domain.Enums;
using System;

namespace gestionMissionBack.Application.DTOs.Incident
{
    public class IncidentDto
    {
        public int IncidentId { get; set; }
        public int MissionId { get; set; }
        public IncidentType Type { get; set; }
        public string Description { get; set; }
        public DateTime ReportDate { get; set; }
        public IncidentStatus Status { get; set; }
    }
}