using gestionMissionBack.Application.DTOs.Document;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentGetDto> GetDocumentByIdAsync(int id);
        Task<IEnumerable<DocumentGetDto>> GetAllDocumentsAsync();
        Task<IEnumerable<DocumentGetDto>> GetDocumentsByTaskIdAsync(int taskId);
        Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto createDto);
        Task UpdateDocumentAsync(DocumentDto documentDto);
        Task DeleteDocumentAsync(int id);
        Task<int> GetDocumentCountByTaskIdAsync(int taskId);
        Task<DocumentDto> CreateDocumentWithFileAsync(DocumentCreateDto createDto);
    }
}