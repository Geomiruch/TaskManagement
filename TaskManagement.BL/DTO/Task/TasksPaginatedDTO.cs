namespace TaskManagement.BL.DTO.Task
{
    public class TasksPaginatedDTO
    {
        public IEnumerable<TaskDTO> Tasks { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalTasks { get; set; }
        public int TotalPages { get; set; }
    }
}
