using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Models;

namespace MyApp.Api.Data
{
    // Inherit from IdentityDbContext to get ASP.NET Core Identity tables
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Post> Posts => Set<Post>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Post table config: simple example with indexes & lengths
            builder.Entity<Post>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Title).HasMaxLength(200).IsRequired();
                entity.Property(p => p.Content).HasMaxLength(10_000).IsRequired();
                entity.Property(p => p.Author).HasMaxLength(100);
                entity.HasIndex(p => p.CreatedAt);
            });
        }
    }
}
