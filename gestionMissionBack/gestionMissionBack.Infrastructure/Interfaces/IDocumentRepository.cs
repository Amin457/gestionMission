using gestionMissionBack.Domain.Entities;


namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface IDocumentRepository : IGenericRepository<Document>
    {
        Task<IEnumerable<Document>> GetDocumentsByTaskIdAsync(int taskId);
        Task<int> GetDocumentCountByTaskIdAsync(int taskId);
    }
}