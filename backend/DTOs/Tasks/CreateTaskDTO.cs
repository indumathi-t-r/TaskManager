using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.DTOs.Tasks
{
    // What the client sends when creating a new task.
    // Example JSON: { "title": "Buy groceries", "description": "Milk and eggs" }
    // Notice: NO UserId here! We get UserId from the JWT token - the user can't fake it.
    public class CreateTaskDTO
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be 1-200 characters")]
        public string Title { get; set; } = string.Empty;

        // Description is optional (no [Required]), so the client can omit it
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
    }
}
