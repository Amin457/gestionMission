using gestionMissionBack.Domain.Enums;
using System;

namespace gestionMissionBack.Application.DTOs.MissionCost
{
    public class MissionCostFilter
    {
        public int? MissionId { get; set; }
        public MissionCostType? Type { get; set; }
        public double? MinAmount { get; set; }
        public double? MaxAmount { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
    }
} 