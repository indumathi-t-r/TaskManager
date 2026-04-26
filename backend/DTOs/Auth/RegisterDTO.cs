using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.DTOs.Auth
{
    // This is what the client sends in the request body when registering.
    // Example JSON: { "username": "Alice", "email": "alice@example.com", "password": "Secret123!" }
    public class RegisterDTO
    {
        // [Required] means this field MUST be provided - ASP.NET validates this automatically
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Username must be 2-50 characters")]
        public string Username { get; set; } = string.Empty;

        // [EmailAddress] validates that it looks like an email (has @ and a domain)
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        // [MinLength] ensures passwords are at least 6 characters
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
    }
}
