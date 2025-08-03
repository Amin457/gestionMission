using System;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.DTOs.MissionCost
{
    public class MissionCostDto
    {
        public int CostId { get; set; }
        public int MissionId { get; set; }
        public MissionCostType Type { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
    }
}