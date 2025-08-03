using gestionMissionBack.Application.DTOs.Article;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface IArticleService
    {
        Task<ArticleGetDto> GetArticleByIdAsync(int id);
        Task<IEnumerable<ArticleGetDto>> GetAllArticlesAsync();
        Task<ArticleGetDto> CreateArticleAsync(ArticleCreateDto articleCreateDto);
        Task UpdateArticleAsync(ArticleUpdateDto articleUpdateDto);
        Task DeleteArticleAsync(int id);
        Task<PagedResult<ArticleGetDto>> GetPagedAsync(int pageNumber, int pageSize, ArticleFilter filter = null);
    }
}