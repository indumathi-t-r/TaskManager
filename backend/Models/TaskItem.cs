namespace TaskManagerAPI.Models
{
    // We name it "TaskItem" (not "Task") because "Task" is already a built-in
    // C# keyword used for async programming. Naming conflicts cause compiler errors.
    public class TaskItem
    {
        // Primary key
        public int Id { get; set; }

        // The task's title, e.g. "Buy groceries"
        public string Title { get; set; } = string.Empty;

        // Optional longer description
        public string? Description { get; set; }  // "?" means this can be null

        // Whether the task is done or not. Defaults to false (not done).
        public bool IsCompleted { get; set; } = false;

        // When this task was created - useful for sorting
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key - links this task to a specific user
        // e.g. UserId = 3 means "this task belongs to the user with Id 3"
        public int UserId { get; set; }

        // Navigation property - gives us access to the full User object
        // The "?" means it might be null when loaded from DB without Include()
        public User? User { get; set; }
    }
}
