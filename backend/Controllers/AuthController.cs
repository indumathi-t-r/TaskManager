using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs.Auth;
using TaskManagerAPI.Models;
using TaskManagerAPI.Services;

namespace TaskManagerAPI.Controllers
{
    // [ApiController] enables automatic model validation, automatic 400 responses,
    // and other API-friendly behaviors. Always add this to Web API controllers.
    [ApiController]

    // [Route("api/auth")] means all endpoints in this controller start with /api/auth
    // [controller] is a placeholder that becomes the controller name minus "Controller"
    // So "AuthController" becomes "auth" → /api/auth
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // _context gives us direct database access via Entity Framework Core
        private readonly AppDbContext _context;

        // _authService gives us HashPassword, VerifyPassword, and GenerateToken
        private readonly IAuthService _authService;

        // Constructor Injection: ASP.NET Core automatically provides these
        // because we registered them in Program.cs with AddDbContext and AddScoped
        public AuthController(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // POST /api/auth/register
        // The [HttpPost("register")] attribute means: "this method handles POST requests to /api/auth/register"
        // [FromBody] tells ASP.NET to read the request body JSON and map it to RegisterDTO
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            // ASP.NET Core already validated [Required] and [EmailAddress] attributes.
            // If validation failed, it returned 400 automatically before reaching here.

            // Check if a user with this email already exists.
            // AnyAsync is more efficient than loading the whole user - it just checks existence.
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email.ToLower());

            if (emailExists)
            {
                // BadRequest returns HTTP 400 with our custom error message
                return BadRequest(new { message = "An account with this email already exists." });
            }

            // Create the new User object.
            // We hash the password - NEVER store plain text passwords.
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email.ToLower(),   // Normalize email to lowercase for consistency
                PasswordHash = _authService.HashPassword(dto.Password)
            };

            // Add to the DbContext's change tracker (not saved to DB yet)
            _context.Users.Add(user);

            // SaveChangesAsync executes the INSERT SQL and saves to the database
            await _context.SaveChangesAsync();

            // Return 201 Created with basic user info.
            // We do NOT return the password hash or any sensitive data.
            // CreatedAtAction also sets the Location header to the created resource's URL.
            return CreatedAtAction(
                nameof(Register),
                new { id = user.Id },
                new
                {
                    message = "Registration successful!",
                    userId = user.Id,
                    username = user.Username,
                    email = user.Email
                }
            );
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            // Find the user by email.
            // FirstOrDefaultAsync returns null if not found (no exception thrown).
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());

            // SECURITY NOTE: We return the SAME error message whether the email doesn't exist
            // OR the password is wrong. This prevents "email enumeration" attacks where
            // an attacker tries emails to find out which ones are registered.
            if (user == null || !_authService.VerifyPassword(dto.Password, user.PasswordHash))
            {
                // Unauthorized returns HTTP 401
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Generate a JWT token for this user
            var token = _authService.GenerateToken(user);

            // Return the token along with user info.
            // The client must save this token and send it in the Authorization header
            // on every subsequent request to protected endpoints.
            return Ok(new
            {
                message = "Login successful!",
                token = token,
                userId = user.Id,
                username = user.Username,
                email = user.Email,
                expiresIn = "24 hours"
            });
        }
    }
}
