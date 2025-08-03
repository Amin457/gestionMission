using Microsoft.AspNetCore.Http;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.DTOs.Document
{
    public class DocumentCreateDto
    {
        public int TaskId { get; set; }
        public string Name { get; set; }
        public DocumentType Type { get; set; }
        public IFormFile File { get; set; }
    }
} 