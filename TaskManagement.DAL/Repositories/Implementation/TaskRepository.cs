using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Models.Enums;

namespace TaskManagement.DAL.Repositories.Implementation
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Domain.Models.Task>> GetAllAsync()
        {
            return await _context.Tasks.ToListAsync();
        }

        public async Task<Domain.Models.Task?> GetByIdAsync(Guid id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<IEnumerable<Domain.Models.Task>> GetTasksByUserIdAsync(Guid userId)
        {
            return await _context.Tasks.Where(t => t.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Domain.Models.Task>> GetFilteredTasksAsync(Guid userId, Status? status, DateTime? dueDate, Priority? priority)
        {
            var query = _context.Tasks.AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status);
            if (dueDate.HasValue)
                query = query.Where(t => t.DueDate == dueDate);
            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority);

            return await query.Where(t => t.UserId == userId).ToListAsync();
        }

        public async Task AddAsync(Domain.Models.Task entity)
        {
            await _context.Tasks.AddAsync(entity);
        }

        public void Update(Domain.Models.Task entity)
        {
            _context.Tasks.Update(entity);
        }

        public void Delete(Domain.Models.Task entity)
        {
            _context.Tasks.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
