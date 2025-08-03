using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Article
{
    public class ArticleCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public List<IFormFile> Photos { get; set; } = new List<IFormFile>(); // Photos to upload
    }
} 