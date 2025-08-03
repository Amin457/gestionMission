using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.DTOs.Document
{
    public class DocumentDto
    {
        public int DocumentId { get; set; }
        public int TaskId { get; set; }
        public string Name { get; set; }
        public DocumentType Type { get; set; }
        public string StoragePath { get; set; }
        public DateTime AddedDate { get; set; }
        // For update
        public string KeptFile { get; set; } // Existing file name/path to keep
        public IFormFile NewFile { get; set; } // New file to upload (optional)
    }
}
