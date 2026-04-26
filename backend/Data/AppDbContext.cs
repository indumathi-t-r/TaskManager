using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Data
{
    // AppDbContext inherits from DbContext (from Entity Framework Core).
    // This class is our gateway to the database.
    public class AppDbContext : DbContext
    {
        // The constructor receives options (like the connection string) via
        // Dependency Injection. We pass them up to the base DbContext class.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet<T> = a table in the database.
        // "Users" will become the table name. You can query it like:
        //   _context.Users.Where(u => u.Email == "test@example.com")
        public DbSet<User> Users { get; set; }

        // "Tasks" will become the Tasks table in the database.
        public DbSet<TaskItem> Tasks { get; set; }

        // OnModelCreating lets us customize how EF Core creates the database schema.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make email unique - no two users can share the same email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Define the relationship: one User has many TaskItems
            // If a user is deleted, delete all their tasks too (cascade delete)
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.User)             // A task has one User
                .WithMany(u => u.Tasks)          // A user has many Tasks
                .HasForeignKey(t => t.UserId)    // The foreign key is UserId
                .OnDelete(DeleteBehavior.Cascade); // Delete tasks when user is deleted
        }
    }
}
