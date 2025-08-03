using gestionMissionBack.Application.DTOs.Article;
using gestionMissionBack.Application.DTOs.TaskMission;
using gestionMissionBack.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface ITaskMissionService
    {
        Task<TaskMissionDto> GetTaskByIdAsync(int id);
        Task<IEnumerable<TaskMissionDto>> GetAllTasksAsync();
        Task<IEnumerable<TaskMissionDto>> GetTasksByMissionIdAsync(int missionId);
        Task<IEnumerable<TaskMissionDtoGet>> GetTasksByMissionIdGetAsync(int missionId);
        Task<TaskMissionDto> CreateTaskAsync(TaskMissionDto taskDto);
        Task UpdateTaskAsync(TaskMissionDto taskDto);
        Task DeleteTaskAsync(int id);
        Task<bool> UpdateTaskStatusAsync(int taskId, TasksStatus newStatus);
        Task<IEnumerable<TaskMissionDto>> GetTasksByStatusAsync(TasksStatus status);
        Task<int> GetTaskCountByMissionIdAsync(int missionId);


        Task AssignArticlesToTaskAsync(int taskId, List<int> articleIds);
        Task RemoveArticlesFromTaskAsync(int taskId, List<int> articleIds);
        Task<IEnumerable<ArticleGetDto>> GetArticlesByTaskIdAsync(int taskId);
    }
}