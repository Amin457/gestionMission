using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskMission = gestionMissionBack.Domain.Entities.TaskMission;

namespace gestionMissionBack.Infrastructure.Repositories
{
    public class TaskMissionRepository : GenericRepository<TaskMission> , ITaskMissionRepository
    {
        private readonly MissionFleetContext _context;

        public TaskMissionRepository(MissionFleetContext context) : base(context)
        {
            _context = context;
        }

        // ITaskRepository specific implementations
        public async Task<IEnumerable<TaskMission>> GetTasksByMissionIdAsync(int missionId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.MissionId == missionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskMission>> GetTasksByStatusAsync(TasksStatus status)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskMission>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.AssignmentDate >= startDate && t.AssignmentDate <= endDate)
                .ToListAsync();
        }

        public async Task<int> GetTaskCountByMissionIdAsync(int missionId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .CountAsync(t => t.MissionId == missionId);
        }

        public async Task<bool> AddArticlesToTaskAsync(int taskId, List<int> articleIds)
        {
            var task = await _context.Tasks
                .Include(t => t.Articles)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
            {
                return false;
            }

            foreach (var articleId in articleIds)
            {
                var article = await _context.Articles.FindAsync(articleId);
                if (article == null)
                {
                    continue; // Skip non-existent articles
                }

                // Check if the article is already associated
                if (!task.Articles.Any(a => a.ArticleId == articleId))
                {
                    task.Articles.Add(article);
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveArticlesFromTaskAsync(int taskId, List<int> articleIds)
        {
            var task = await _context.Tasks
                .Include(t => t.Articles)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
            {
                return false;
            }

            var articlesToRemove = task.Articles
                .Where(a => articleIds.Contains(a.ArticleId))
                .ToList();

            if (!articlesToRemove.Any())
            {
                return true; // No articles to remove, operation is successful
            }

            foreach (var article in articlesToRemove)
            {
                task.Articles.Remove(article);
            }

            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<IEnumerable<Article>> GetArticlesByTaskIdAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.Articles)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
            {
                return new List<Article>();
            }

            return task.Articles;
        }
    }
}