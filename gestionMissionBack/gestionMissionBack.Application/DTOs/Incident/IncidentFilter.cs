using gestionMissionBack.Domain.Enums;
using System;

namespace gestionMissionBack.Application.DTOs.Incident
{
    public class IncidentFilter
    {
        public int? MissionId { get; set; }
        public IncidentType? Type { get; set; }
        public IncidentStatus? Status { get; set; }
        public DateTime? ReportDateStart { get; set; }
        public DateTime? ReportDateEnd { get; set; }
    }
} 