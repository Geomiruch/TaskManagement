using TaskManagement.Domain.Models.Enums;

namespace TaskManagement.DAL.Repositories
{
    public interface ITaskRepository : IRepository<Domain.Models.Task>
    {
        Task<IEnumerable<Domain.Models.Task>> GetTasksByUserIdAsync(Guid userId);
        Task<IEnumerable<Domain.Models.Task>> GetFilteredTasksAsync(Guid userId, Status? status, DateTime? dueDate, Priority? priority);
    }
}
