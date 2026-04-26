namespace TaskManagerAPI.Models
{
    // This class maps directly to a "Users" table in the database.
    // Entity Framework Core will create this table for us when we run migrations.
    public class User
    {
        // Primary key - EF Core automatically makes "Id" the primary key
        public int Id { get; set; }

        // The user's display name (e.g. "Alice")
        public string Username { get; set; } = string.Empty;

        // The user's email - used for login
        public string Email { get; set; } = string.Empty;

        // NEVER store plain-text passwords. We store a bcrypt hash.
        // Example hash: "$2a$11$abc123..." - impossible to reverse
        public string PasswordHash { get; set; } = string.Empty;

        // Navigation property - one User has MANY TaskItems
        // EF Core uses this to set up the one-to-many relationship
        // "= new List<TaskItem>()" prevents null reference errors
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
