using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.DTOs.MissionCost
{
    public class MissionCostCreateDto
    {
        public int MissionId { get; set; }
        public MissionCostType Type { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public List<IFormFile>? ReceiptPhotos { get; set; }
    }
} 