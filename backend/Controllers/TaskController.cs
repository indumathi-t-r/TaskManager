using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs.Tasks;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    // [Authorize] on the entire controller means: EVERY endpoint in this controller
    // requires a valid JWT token. If you don't have one, you get HTTP 401 Unauthorized.
    // This protects all task endpoints automatically - no need to add it to each method.
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        // This is a helper method to get the current user's ID from the JWT token.
        // Remember: when you log in, we embedded the user's ID as a Claim in the token.
        // ASP.NET Core's middleware already validated the token and populated User.Claims.
        // We just need to read the claim here.
        private int GetCurrentUserId()
        {
            // ClaimTypes.NameIdentifier = the claim we set to user.Id.ToString() in AuthService
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Parse it from string to int
            return int.Parse(userIdClaim!);
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/task  → Get all tasks for the logged-in user
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var userId = GetCurrentUserId();

            // Query only tasks belonging to the current user.
            // IMPORTANT: We filter by userId - users can ONLY see their own tasks.
            // OrderByDescending shows newest tasks first.
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new  // "Select" maps the TaskItem to an anonymous object (our response shape)
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.IsCompleted,
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/task/{id}  → Get a single task by ID
        // {id} in the route is a route parameter - maps to the "id" parameter below
        // ─────────────────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var userId = GetCurrentUserId();

            // Find the task - but ALSO check it belongs to the current user!
            // Without the userId check, User A could access User B's task by guessing the ID.
            // This is called an "Insecure Direct Object Reference" (IDOR) vulnerability.
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                // NotFound returns HTTP 404
                return NotFound(new { message = "Task not found." });
            }

            return Ok(new
            {
                task.Id,
                task.Title,
                task.Description,
                task.IsCompleted,
                task.CreatedAt
            });
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/task  → Create a new task
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDTO dto)
        {
            var userId = GetCurrentUserId();

            // Build the new task. We set UserId from the JWT, not from the request body.
            // The user cannot lie about which user they are - the token is cryptographically signed.
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                IsCompleted = false,     // New tasks start as not completed
                CreatedAt = DateTime.UtcNow,
                UserId = userId          // Securely set from the JWT token
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // 201 Created with the location of the new resource
            return CreatedAtAction(
                nameof(GetTaskById),
                new { id = task.Id },
                new
                {
                    message = "Task created successfully!",
                    task.Id,
                    task.Title,
                    task.Description,
                    task.IsCompleted,
                    task.CreatedAt
                }
            );
        }

        // ─────────────────────────────────────────────────────────
        // PUT /api/task/{id}  → Update an entire task (full replacement)
        // PUT = replace everything. PATCH = update only specific fields.
        // For simplicity, we use PUT here.
        // ─────────────────────────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDTO dto)
        {
            var userId = GetCurrentUserId();

            // Find the task and verify ownership
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                return NotFound(new { message = "Task not found." });
            }

            // Update the task's properties with values from the DTO
            task.Title = dto.Title;
            task.Description = dto.Description;
            task.IsCompleted = dto.IsCompleted;

            // EF Core's change tracker knows this entity was modified.
            // SaveChangesAsync will generate an UPDATE SQL statement.
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Task updated successfully!",
                task.Id,
                task.Title,
                task.Description,
                task.IsCompleted,
                task.CreatedAt
            });
        }

        // ─────────────────────────────────────────────────────────
        // DELETE /api/task/{id}  → Delete a task
        // ─────────────────────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetCurrentUserId();

            // Find the task and verify ownership
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                return NotFound(new { message = "Task not found." });
            }

            // Mark the entity for deletion
            _context.Tasks.Remove(task);

            // Execute the DELETE SQL
            await _context.SaveChangesAsync();

            // NoContent returns HTTP 204 - success but no response body
            // This is the standard response for DELETE operations
            return NoContent();
        }
    }
}
