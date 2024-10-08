﻿using TaskManagement.Domain.Models.Enums;

namespace TaskManagement.BL.DTO.Task
{
    public class TaskDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public Status Status { get; set; }
        public Priority Priority { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
