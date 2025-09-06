using System.ComponentModel.DataAnnotations;

namespace MyApp.Api.DTOs
{
    public class RegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(32)]
        public string Username { get; set; } = string.Empty;

        // NOTE: In production consider stronger rules (min length 12, non-dictionary, etc.)
        [Required, MinLength(8), MaxLength(64)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
