using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Persistence;
using gestionMissionBack.Domain.DTOs;
using gestionMissionBack.Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestionMissionBack.Infrastructure.Repositories
{
    public class ArticleRepository : GenericRepository<Article>, IArticleRepository
    {
        private readonly MissionFleetContext _context;

        public ArticleRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PagedResult<Article>> GetPagedAsync(int pageNumber, int pageSize, ArticleFilter filter = null)
        {
            var query = _context.Articles.AsNoTracking();

            // Apply filters if provided
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Name))
                    query = query.Where(a => a.Name.Contains(filter.Name));

                if (!string.IsNullOrEmpty(filter.Description))
                    query = query.Where(a => a.Description.Contains(filter.Description));

                if (filter.MinQuantity.HasValue)
                    query = query.Where(a => a.Quantity >= filter.MinQuantity.Value);

                if (filter.MaxQuantity.HasValue)
                    query = query.Where(a => a.Quantity <= filter.MaxQuantity.Value);

                if (filter.MinWeight.HasValue)
                    query = query.Where(a => a.Weight >= filter.MinWeight.Value);

                if (filter.MaxWeight.HasValue)
                    query = query.Where(a => a.Weight <= filter.MaxWeight.Value);

                if (filter.MinVolume.HasValue)
                    query = query.Where(a => a.Volume >= filter.MinVolume.Value);

                if (filter.MaxVolume.HasValue)
                    query = query.Where(a => a.Volume <= filter.MaxVolume.Value);
            }

            var totalRecords = await query.CountAsync();

            var articles = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Article>
            {
                Data = articles,
                TotalRecords = totalRecords
            };
        }
    }
}