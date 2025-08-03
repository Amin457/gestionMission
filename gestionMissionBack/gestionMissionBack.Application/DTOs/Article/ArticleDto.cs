using System;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Article
{
    public class ArticleDto
    {
        public int ArticleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public List<string> PhotoUrls { get; set; } = new List<string>();
    }
}