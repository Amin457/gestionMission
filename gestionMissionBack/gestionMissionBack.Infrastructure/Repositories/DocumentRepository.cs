using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestionMissionBack.Infrastructure.Repositories
{
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        private readonly MissionFleetContext _context;

        public DocumentRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Document>> GetDocumentsByTaskIdAsync(int taskId)
        {
            return await _context.Documents
                .AsNoTracking()
                .Where(d => d.TaskId == taskId)
                .ToListAsync();
        }

        public async Task<int> GetDocumentCountByTaskIdAsync(int taskId)
        {
            return await _context.Documents
                .AsNoTracking()
                .CountAsync(d => d.TaskId == taskId);
        }
    }
}