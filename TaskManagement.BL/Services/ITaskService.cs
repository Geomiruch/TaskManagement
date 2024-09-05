using TaskManagement.BL.DTO.Task;

namespace TaskManagement.BL.Services
{
    public interface ITaskService
    {
        Task<Domain.Models.Task> CreateTaskAsync(TaskDTO taskDto, Guid userId);
        Task<TasksPaginatedDTO> GetTasksAsync(TaskFilterDTO filterDto, int page, int pageSize);
        Task<Domain.Models.Task?> GetTaskByIdAsync(Guid taskId, Guid userId);
        Task<bool> UpdateTaskAsync(Domain.Models.Task task);
        Task<bool> DeleteTaskAsync(Guid taskId, Guid userId);
    }
}
