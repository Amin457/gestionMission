using gestionMissionBack.Application.DTOs.Article;
using gestionMissionBack.Application.DTOs.TaskMission;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskMissionsController : ControllerBase
    {
        private readonly ITaskMissionService _taskMissionService;

        public TaskMissionsController(ITaskMissionService taskMissionService)
        {
            _taskMissionService = taskMissionService ?? throw new ArgumentNullException(nameof(taskMissionService));
        }

        // GET: api/taskmissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskMissionDto>>> GetAllTasks()
        {
            var tasks = await _taskMissionService.GetAllTasksAsync();
            return Ok(tasks);
        }

        // GET: api/taskmissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskMissionDto>> GetTask(int id)
        {
            var task = await _taskMissionService.GetTaskByIdAsync(id);
            return Ok(task);
        }

        // GET: api/taskmissions/mission/5
        [HttpGet("mission/{missionId}")]
        public async Task<ActionResult<IEnumerable<TaskMissionDtoGet>>> GetTasksByMissionId(int missionId)
        {
            var tasks = await _taskMissionService.GetTasksByMissionIdGetAsync(missionId);
            return Ok(tasks);
        }

        // GET: api/taskmissions/status/pending
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<TaskMissionDto>>> GetTasksByStatus(TasksStatus status)
        {
            var tasks = await _taskMissionService.GetTasksByStatusAsync(status);
            return Ok(tasks);
        }

        // GET: api/taskmissions/count/mission/5
        [HttpGet("count/mission/{missionId}")]
        public async Task<ActionResult<int>> GetTaskCountByMissionId(int missionId)
        {
            var count = await _taskMissionService.GetTaskCountByMissionIdAsync(missionId);
            return Ok(count);
        }

        // POST: api/taskmissions
        [HttpPost]
        public async Task<ActionResult<TaskMissionDto>> CreateTask([FromBody] TaskMissionDto taskDto)
        {
            var createdTask = await _taskMissionService.CreateTaskAsync(taskDto);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.TaskId }, createdTask);
        }

        // PUT: api/taskmissions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskMissionDto taskDto)
        {
            if (id != taskDto.TaskId)
            {
                throw new ArgumentException("ID in URL must match ID in body");
            }

            await _taskMissionService.UpdateTaskAsync(taskDto);
            return Ok();
        }

        // DELETE: api/taskmissions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            await _taskMissionService.DeleteTaskAsync(id);
            return NoContent();
        }

        // PATCH: api/taskmissions/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] TasksStatus newStatus)
        {
            var success = await _taskMissionService.UpdateTaskStatusAsync(id, newStatus);
            if (!success)
            {
                return NotFound($"Task with ID {id} not found");
            }
            return Ok();
        }

        // POST: api/taskmissions/5/articles
        [HttpPost("{taskId}/articles")]
        public async Task<IActionResult> AssignArticlesToTask(int taskId, [FromBody] List<int> ArticleIds)
        {
            if (ArticleIds == null || !ArticleIds.Any())
            {
                throw new ArgumentException("Article IDs list cannot be null or empty");
            }

            await _taskMissionService.AssignArticlesToTaskAsync(taskId, ArticleIds);
            return NoContent();
        }

        // DELETE: api/taskmissions/5/articles
        [HttpDelete("{taskId}/articles")]
        public async Task<IActionResult> RemoveArticlesFromTask(int taskId, [FromBody] List<int> ArticleIds)
        {
            if (ArticleIds == null || !ArticleIds.Any())
            {
                throw new ArgumentException("Article IDs list cannot be null or empty");
            }

            await _taskMissionService.RemoveArticlesFromTaskAsync(taskId, ArticleIds);
            return NoContent();
        }

        // GET: api/taskmissions/5/articles
        [HttpGet("{taskId}/articles")]
        public async Task<ActionResult<IEnumerable<ArticleGetDto>>> GetArticlesByTaskId(int taskId)
        {
            var articles = await _taskMissionService.GetArticlesByTaskIdAsync(taskId);
            return Ok(articles);
        }
    }
}