using System;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.DTOs.Document
{
    public class DocumentGetDto
    {
        public int DocumentId { get; set; }
        public int TaskId { get; set; }
        public string Name { get; set; }
        public DocumentType Type { get; set; }
        public string StoragePath { get; set; }
        public DateTime AddedDate { get; set; }
    }
} 