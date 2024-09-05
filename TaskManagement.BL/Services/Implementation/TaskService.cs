using AutoMapper;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TaskManagement.BL.DTO.Task;
using TaskManagement.DAL.Repositories;

namespace TaskManagement.BL.Services.Implementation
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<TaskService> _logger;
        private readonly IMapper _mapper;

        public TaskService(ITaskRepository taskRepository, ILogger<TaskService> logger, IMapper mapper)
        {
            _taskRepository = taskRepository;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<TasksPaginatedDTO> GetTasksAsync(TaskFilterDTO taskFilter, int page, int pageSize)
        {
            var tasks = await _taskRepository.GetFilteredTasksAsync(taskFilter.UserId, taskFilter.Status, taskFilter.DueDate, taskFilter.Priority);
            var tasksDTO = tasks.Select(_mapper.Map<TaskDTO>);
            if(taskFilter.DueDateOrder.HasValue)
            {
                if (taskFilter.DueDateOrder == SortOrder.Ascending)
                    tasksDTO.OrderBy(x =>  x.DueDate);
                if (taskFilter.DueDateOrder == SortOrder.Descending)
                    tasksDTO.OrderByDescending(x => x.DueDate);
            }
            if (taskFilter.PriorityOrder.HasValue)
            {
                if (taskFilter.PriorityOrder == SortOrder.Ascending)
                    tasksDTO.OrderBy(x => x.Priority);
                if (taskFilter.PriorityOrder == SortOrder.Descending)
                    tasksDTO.OrderByDescending(x => x.Priority);
            }

            var totalItems = tasksDTO.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var paginatedTasks = tasksDTO
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new TasksPaginatedDTO
            {
                Tasks = paginatedTasks,
                Page = page,
                PageSize = pageSize,
                TotalTasks = totalItems,
                TotalPages = totalPages
            };
        }

        public async Task<Domain.Models.Task?> GetTaskByIdAsync(Guid taskId, Guid userId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            return task?.UserId == userId ? task : null;
        }

        public async Task<Domain.Models.Task> CreateTaskAsync(TaskDTO taskDTO, Guid userId)
        {
            var task = _mapper.Map<Domain.Models.Task>(taskDTO);
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;
            task.UserId = userId;

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            _logger.LogInformation("Task {Title} created by user {UserId}", taskDTO.Title, userId);
            return task;
        }

        public async Task<bool> DeleteTaskAsync(Guid taskId, Guid userId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null || task.UserId != userId)
            {
                _logger.LogWarning("Attempt to delete task {TaskId} by unauthorized user {UserId}", taskId, userId);
                return false;
            }

            _taskRepository.Delete(task);
            await _taskRepository.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} deleted by user {UserId}", taskId, userId);
            return true;
        }

        public async Task<bool> UpdateTaskAsync(Domain.Models.Task updatedTask)
        {
            var task = await _taskRepository.GetByIdAsync(updatedTask.Id);
            if (task == null)
            {
                _logger.LogInformation("Task {TaskId} not found", updatedTask.Id);
                return false;
            }
            if (task.UserId != updatedTask.UserId)
            {
                _logger.LogInformation("Authorization failed");
                return false;
            }

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.DueDate = updatedTask.DueDate;
            task.Status = updatedTask.Status;
            task.Priority = updatedTask.Priority;

            task.UpdatedAt = DateTime.UtcNow;

            _taskRepository.Update(task);
            await _taskRepository.SaveChangesAsync();
            return true;
        }

    }
}
