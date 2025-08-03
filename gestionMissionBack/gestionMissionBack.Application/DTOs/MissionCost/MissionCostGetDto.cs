using System;
using System.Collections.Generic;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.DTOs.MissionCost
{
    public class MissionCostGetDto
    {
        public int CostId { get; set; }
        public int MissionId { get; set; }
        public MissionCostType Type { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public List<string>? ReceiptPhotoUrls { get; set; }
    }
} 