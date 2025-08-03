using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Domain.DTOs;

namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface IArticleRepository : IGenericRepository<Article>
    {
        Task<PagedResult<Article>> GetPagedAsync(int pageNumber, int pageSize, ArticleFilter filter = null);
    }
}