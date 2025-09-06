using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.DTOs;
using MyApp.Api.Models;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PostsController(AppDbContext db) { _db = db; }

        // GET /api/posts?skip=0&take=20
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Post>>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            take = Math.Clamp(take, 1, 100);
            var items = await _db.Posts
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
            return Ok(items);
        }

        // GET /api/posts/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Post>> GetOne([FromRoute] int id)
        {
            var post = await _db.Posts.FindAsync(id);
            if (post == null) return NotFound();
            return Ok(post);
        }

        // POST /api/posts
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Post>> Create([FromBody] PostCreateRequest req)
        {
            var post = new Post
            {
                Title = req.Title,
                Content = req.Content,
                Author = string.IsNullOrWhiteSpace(req.Author) ? User.Identity?.Name : req.Author,
                CreatedAt = DateTime.UtcNow
            };

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { id = post.Id }, post);
        }

        // PUT /api/posts/5
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<ActionResult<Post>> Update([FromRoute] int id, [FromBody] PostUpdateRequest req)
        {
            var post = await _db.Posts.FindAsync(id);
            if (post == null) return NotFound();

            post.Title = req.Title;
            post.Content = req.Content;
            post.Author = string.IsNullOrWhiteSpace(req.Author) ? post.Author : req.Author;
            post.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(post);
        }

        // DELETE /api/posts/5
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var post = await _db.Posts.FindAsync(id);
            if (post == null) return NotFound();

            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
