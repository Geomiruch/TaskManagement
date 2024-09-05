using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TaskManagement.Domain.Models.Enums;

namespace TaskManagement.BL.DTO.Task
{
    public class TaskFilterDTO
    {
        public Guid UserId { get; set; }
        public Status? Status { get; set; }
        public Priority? Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public SortOrder? PriorityOrder { get; set; }
        public SortOrder? DueDateOrder { get; set; }
    }
}
