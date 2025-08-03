using System;
using System.Collections.Generic;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.DTOs.Incident
{
    public class IncidentGetDto
    {
        public int IncidentId { get; set; }
        public int MissionId { get; set; }
        public IncidentType Type { get; set; }
        public string Description { get; set; }
        public DateTime ReportDate { get; set; }
        public IncidentStatus Status { get; set; }
        public List<string>? IncidentDocsUrls { get; set; }
    }
} 