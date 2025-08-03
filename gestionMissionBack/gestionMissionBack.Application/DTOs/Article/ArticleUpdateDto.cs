using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace gestionMissionBack.Application.DTOs.Article
{
    public class ArticleUpdateDto
    {
        public int ArticleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public double Volume { get; set; }
        public List<string> KeepPhotosUrls { get; set; } = new List<string>(); // Existing photo URLs to keep
        public List<IFormFile> NewPhotos { get; set; } = new List<IFormFile>(); // New photos to upload
    }
} 