using TaskManagement.BL.DTO.Task;

namespace TaskManagement.BL.Services
{
    public interface ITaskService
    {
        Task<Domain.Models.Task> CreateTaskAsync(TaskDTO taskDto);
        Task<IEnumerable<TaskDTO>> GetTasksAsync(TaskFilterDTO filterDto);
        Task<Domain.Models.Task?> GetTaskByIdAsync(Guid taskId, Guid userId);
        Task<bool> UpdateTaskAsync(Domain.Models.Task task);
        Task<bool> DeleteTaskAsync(Guid taskId, Guid userId);
    }
}
