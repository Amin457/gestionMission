
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Enums;
using TaskMission = gestionMissionBack.Domain.Entities.TaskMission;

namespace gestionMissionBack.Infrastructure.Interfaces
{
    public interface ITaskMissionRepository : IGenericRepository<TaskMission>
    {
        // Additional task-specific methods
        Task<IEnumerable<TaskMission>> GetTasksByMissionIdAsync(int missionId);
        Task<IEnumerable<TaskMission>> GetTasksByStatusAsync(TasksStatus status);
        Task<IEnumerable<TaskMission>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTaskCountByMissionIdAsync(int missionId);
        Task<bool> AddArticlesToTaskAsync(int taskId, List<int> articleIds);
        Task<bool> RemoveArticlesFromTaskAsync(int taskId, List<int> articleIds);
        Task<IEnumerable<Article>> GetArticlesByTaskIdAsync(int taskId);

    }
}