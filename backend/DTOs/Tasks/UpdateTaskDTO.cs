using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.DTOs.Tasks
{
    // What the client sends when updating an existing task.
    // Example JSON: { "title": "Buy groceries", "description": "Also get bread", "isCompleted": true }
    public class UpdateTaskDTO
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        // The client can mark a task as complete/incomplete
        public bool IsCompleted { get; set; }
    }
}
