using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.DTOs.Incident
{
    public class IncidentUpdateDto
    {
        public int MissionId { get; set; }
        public IncidentType Type { get; set; }
        public string Description { get; set; }
        public DateTime ReportDate { get; set; }
        public IncidentStatus Status { get; set; }
        public List<IFormFile>? IncidentDocs { get; set; }
    }
} 