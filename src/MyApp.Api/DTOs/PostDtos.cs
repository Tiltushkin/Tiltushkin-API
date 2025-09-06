using System.ComponentModel.DataAnnotations;

namespace MyApp.Api.DTOs
{
    public class PostCreateRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(10_000)]
        public string Content { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Author { get; set; }
    }

    public class PostUpdateRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(10_000)]
        public string Content { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Author { get; set; }
    }
}
