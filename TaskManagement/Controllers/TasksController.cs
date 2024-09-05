using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManagement.BL.DTO.Task;
using TaskManagement.BL.Services;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] TaskFilterDTO taskFilter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            taskFilter.UserId = GetCurrentUserId();
            var paginatedResult = await _taskService.GetTasksAsync(taskFilter, page, pageSize);

            return Ok(paginatedResult);
        }

        [HttpGet("{taskId}")]
        [Authorize]
        public async Task<IActionResult> GetTaskById(Guid taskId)
        {
            var task = await _taskService.GetTaskByIdAsync(taskId, GetCurrentUserId());
            if (task == null)
            {
                _logger.LogWarning("Task {TaskId} not found for user {UserId}", taskId, GetCurrentUserId());
                return NotFound();
            }
            return Ok(task);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTask([FromBody] TaskDTO taskDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid task data.");
            }

            var userId = GetCurrentUserId();
            await _taskService.CreateTaskAsync(taskDto, userId);
            return Ok();
        }

        [HttpPut("{taskId}")]
        [Authorize]
        public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] TaskDTO taskDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid task data.");
            }

            var taskToUpdate = await _taskService.GetTaskByIdAsync(taskId, GetCurrentUserId());
            if (taskToUpdate == null)
            {
                _logger.LogWarning("Task {TaskId} not found for user {UserId}", taskId, GetCurrentUserId());
                return NotFound();
            }

            var updatedTask = await _taskService.UpdateTaskAsync(new Domain.Models.Task
            {
                Id = taskId,
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                Status = taskDto.Status,
                Priority = taskDto.Priority,
                UserId = GetCurrentUserId()
            });

            if (!updatedTask)
            {
                return BadRequest("Task update failed.");
            }

            return NoContent();
        }

        [HttpDelete("{taskId}")]
        [Authorize]
        public async Task<IActionResult> DeleteTask(Guid taskId)
        {
            var deleted = await _taskService.DeleteTaskAsync(taskId, GetCurrentUserId());
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            string id = string.Empty;
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var idClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);
                id = idClaim.Value;
            }

            return Guid.Parse(id);
        }
    }
}
