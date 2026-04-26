using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.DTOs.Auth
{
    // This is what the client sends when logging in.
    // Example JSON: { "email": "alice@example.com", "password": "Secret123!" }
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
