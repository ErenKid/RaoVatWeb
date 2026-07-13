using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Models;

namespace RaoVatWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostImage> PostImages { get; set; }

        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;

        public DbSet<Conversation> Conversations { get; set; } = null!;
public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    }
}