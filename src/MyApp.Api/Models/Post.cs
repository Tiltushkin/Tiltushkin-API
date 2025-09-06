using System.ComponentModel.DataAnnotations;

namespace MyApp.Api.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(10_000)]
        public string Content { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Author { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
