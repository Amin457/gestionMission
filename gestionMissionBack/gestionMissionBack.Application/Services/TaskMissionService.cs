using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.DTOs.Article;
using gestionMissionBack.Application.DTOs.TaskMission;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Validators;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Enums;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Application.DTOs.Notification;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TaskMission = gestionMissionBack.Domain.Entities.TaskMission;

namespace gestionMissionBack.Application.Services
{
    public class TaskMissionService : ITaskMissionService
    {
        private readonly ITaskMissionRepository _taskRepository;
        private readonly IValidator<TaskMissionDto> _taskValidator;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public TaskMissionService(
            ITaskMissionRepository taskRepository,
            IValidator<TaskMissionDto> taskValidator,
            IMapper mapper,
            INotificationService notificationService)
        {
            _taskRepository = taskRepository ;
            _taskValidator = taskValidator ;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<TaskMissionDto> GetTaskByIdAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            return _mapper.Map<TaskMissionDto>(task);
        }

        public async Task<IEnumerable<TaskMissionDto>> GetAllTasksAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TaskMissionDto>>(tasks);
        }

        public async Task<IEnumerable<TaskMissionDto>> GetTasksByMissionIdAsync(int missionId)
        {
            var tasks = await _taskRepository.GetTasksByMissionIdAsync(missionId);
            return _mapper.Map<IEnumerable<TaskMissionDto>>(tasks);
        }

        public async Task<IEnumerable<TaskMissionDtoGet>> GetTasksByMissionIdGetAsync(int missionId)
        {
            var tasks = await _taskRepository.GetTasksByMissionIdAsync(missionId);
            return _mapper.Map<IEnumerable<TaskMissionDtoGet>>(tasks);
        }

        public async Task<TaskMissionDto> CreateTaskAsync(TaskMissionDto taskDto)
        {
            // Validate the DTO
            var validationResult = await _taskValidator.ValidateAsync(taskDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // If this task is marked as first, unmark any other first task in the mission
            if (taskDto.IsFirstTask)
            {
                var existingFirstTask = await _taskRepository.GetQueryable()
                    .Where(t => t.MissionId == taskDto.MissionId && t.IsFirstTask)
                    .FirstOrDefaultAsync();

                if (existingFirstTask != null)
                {
                    existingFirstTask.IsFirstTask = false;
                    await _taskRepository.UpdateAsync(existingFirstTask);
                }
            }
            // If no task is marked as first in the mission, mark this one as first
            else
            {
                var hasFirstTask = await _taskRepository.GetQueryable()
                    .AnyAsync(t => t.MissionId == taskDto.MissionId && t.IsFirstTask);

                if (!hasFirstTask)
                {
                    taskDto.IsFirstTask = true;
                }
            }

            taskDto.AssignmentDate = DateTime.Now;
            var task = _mapper.Map<TaskMission>(taskDto);
            var createdTask = await _taskRepository.AddAsync(task);
            
            // Get mission details for notification
            var mission = await _taskRepository.GetQueryable()
                .Where(t => t.TaskId == createdTask.TaskId)
                .Include(t => t.Mission)
                .Select(t => t.Mission)
                .FirstOrDefaultAsync();
            
            // Automatic notification to driver when task is created
            if (mission != null)
            {
                await _notificationService.SendRealTimeNotificationAsync(
                    mission.DriverId,
                    new CreateNotificationDto
                    {
                        UserId = mission.DriverId,
                        Title = "New Task Added",
                        Message = $"New task added to mission #{mission.Service}: {createdTask.Description}",
                        NotificationType = NotificationCategory.Task,
                        Priority = NotificationPriority.Normal,
                        RelatedEntityType = "Task",
                        RelatedEntityId = createdTask.TaskId
                    }
                );
            }
            
            return _mapper.Map<TaskMissionDto>(createdTask);
        }

        public async Task UpdateTaskAsync(TaskMissionDto taskDto)
        {
            // Validate the DTO
            var validationResult = await _taskValidator.ValidateAsync(taskDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingTask = await _taskRepository.GetByIdAsync(taskDto.TaskId);
            if (existingTask == null)
            {
                throw new KeyNotFoundException($"Task with ID {taskDto.TaskId} not found");
            }

            // If this task is being marked as first, unmark any other first task
            if (taskDto.IsFirstTask && !existingTask.IsFirstTask)
            {
                var otherFirstTask = await _taskRepository.GetQueryable()
                    .Where(t => t.MissionId == taskDto.MissionId && t.IsFirstTask && t.TaskId != taskDto.TaskId)
                    .FirstOrDefaultAsync();

                if (otherFirstTask != null)
                {
                    otherFirstTask.IsFirstTask = false;
                    await _taskRepository.UpdateAsync(otherFirstTask);
                }
            }
            // If this task is being unmarked as first and it was the first task
            else if (!taskDto.IsFirstTask && existingTask.IsFirstTask)
            {
                // Find another task to mark as first
                var anotherTask = await _taskRepository.GetQueryable()
                    .Where(t => t.MissionId == taskDto.MissionId && t.TaskId != taskDto.TaskId)
                    .FirstOrDefaultAsync();

                if (anotherTask != null)
                {
                    anotherTask.IsFirstTask = true;
                    await _taskRepository.UpdateAsync(anotherTask);
                }
            }

            _mapper.Map(taskDto, existingTask);
            var updated = await _taskRepository.UpdateAsync(existingTask);
            if (!updated)
            {
                throw new Exception($"Failed to update task with ID {taskDto.TaskId}");
            }
        }

        public async Task DeleteTaskAsync(int id)
        {
            var deleted = await _taskRepository.DeleteAsync(id);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Task with ID {id} not found or could not be deleted");
            }
        }

        public async Task<bool> UpdateTaskStatusAsync(int taskId, TasksStatus newStatus)
        {
            var task = await _taskRepository.GetQueryable()
                .Include(t => t.Mission)
                .ThenInclude(m => m.Requester)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);
            
            if (task == null)
                return false;

            var oldStatus = task.Status;
            task.Status = newStatus;
            
            if (newStatus == TasksStatus.Completed)
            {
                task.CompletionDate = DateTime.UtcNow;
            }
            
            await _taskRepository.UpdateAsync(task);
            
            // Automatic notification for status change
            if (oldStatus != newStatus)
            {
                // Notify driver
                await _notificationService.SendRealTimeNotificationAsync(
                    task.Mission.DriverId,
                    new CreateNotificationDto
                    {
                        UserId = task.Mission.DriverId,
                        Title = "Task Status Updated",
                        Message = $"Task #{task.Description} status changed to {newStatus}",
                        NotificationType = NotificationCategory.Task,
                        Priority = NotificationPriority.Normal,
                        RelatedEntityType = "Task",
                        RelatedEntityId = taskId
                    }
                );
                
                // Notify requester if task is completed
                if (newStatus == TasksStatus.Completed)
                {
                    await _notificationService.SendRealTimeNotificationAsync(
                        task.Mission.RequesterId,
                        new CreateNotificationDto
                        {
                            UserId = task.Mission.RequesterId,
                            Title = "Task Completed",
                            Message = $"Task #{task.Description} has been completed for mission #{task.Mission.MissionId}",
                            NotificationType = NotificationCategory.Task,
                            Priority = NotificationPriority.Normal,
                            RelatedEntityType = "Task",
                            RelatedEntityId = taskId
                        }
                    );
                }
            }
            
            return true;
        }

        public async Task<IEnumerable<TaskMissionDto>> GetTasksByStatusAsync(TasksStatus status)
        {
            var tasks = await _taskRepository.GetTasksByStatusAsync(status);
            return _mapper.Map<IEnumerable<TaskMissionDto>>(tasks);
        }

        public async Task<int> GetTaskCountByMissionIdAsync(int missionId)
        {
            return await _taskRepository.GetTaskCountByMissionIdAsync(missionId);
        }

        public async Task AssignArticlesToTaskAsync(int taskId, List<int> articleIds)
        {
            // Validate task existence
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new KeyNotFoundException($"Task with ID {taskId} not found");
            }

            // Validate article IDs
            if (articleIds == null || !articleIds.Any())
            {
                throw new ArgumentException("Article IDs list cannot be null or empty");
            }

            // Remove duplicates and validate article existence (if repository supports it)
            articleIds = articleIds.Distinct().ToList();
            // Note: Ideally, check if articles exist via a repository method like _articleRepository.ExistsAsync
            // For now, assume repository validates article existence

            // Assign articles to task
            var success = await _taskRepository.AddArticlesToTaskAsync(taskId, articleIds);
            if (!success)
            {
                throw new Exception($"Failed to assign articles to task with ID {taskId}");
            }
        }

        public async Task RemoveArticlesFromTaskAsync(int taskId, List<int> articleIds)
        {
            // Validate task existence
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new KeyNotFoundException($"Task with ID {taskId} not found");
            }

            // Validate article IDs
            if (articleIds == null || !articleIds.Any())
            {
                throw new ArgumentException("Article IDs list cannot be null or empty");
            }

            // Remove duplicates
            articleIds = articleIds.Distinct().ToList();

            // Remove articles from task
            var success = await _taskRepository.RemoveArticlesFromTaskAsync(taskId, articleIds);
            if (!success)
            {
                throw new Exception($"Failed to remove articles from task with ID {taskId}");
            }
        }

        public async Task<IEnumerable<ArticleGetDto>> GetArticlesByTaskIdAsync(int taskId)
        {
            var articles = await _taskRepository.GetArticlesByTaskIdAsync(taskId);
            return articles.Select(article => new ArticleGetDto
            {
                ArticleId = article.ArticleId,
                Name = article.Name,
                Description = article.Description,
                Quantity = article.Quantity,
                Weight = article.Weight,
                Volume = article.Volume,
                PhotoUrls = !string.IsNullOrEmpty(article.PhotoUrls) 
                    ? JsonSerializer.Deserialize<List<string>>(article.PhotoUrls) ?? new List<string>()
                    : new List<string>()
            });
        }
    }
}