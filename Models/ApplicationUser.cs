using Microsoft.AspNetCore.Identity;

namespace RaoVatWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? AvatarUrl { get; set; }

        public bool IsVip { get; set; } = false;

        public DateTime? VipExpiredAt { get; set; }

        public bool IsLocked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}