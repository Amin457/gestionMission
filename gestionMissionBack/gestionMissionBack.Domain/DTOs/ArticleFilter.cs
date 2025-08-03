namespace gestionMissionBack.Domain.DTOs
{
    public class ArticleFilter
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
        public double? MinWeight { get; set; }
        public double? MaxWeight { get; set; }
        public double? MinVolume { get; set; }
        public double? MaxVolume { get; set; }
    }
} 