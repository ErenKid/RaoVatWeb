using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RaoVatWeb.Models.Enums;

namespace RaoVatWeb.Models
{
    public class Post
    {
        public int PostId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        public PostType PostType { get; set; } = PostType.Sell;

        public PostStatus Status { get; set; } = PostStatus.Pending;

        [StringLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [StringLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ContactEmail { get; set; }

        public int ViewCount { get; set; } = 0;

        public bool IsVipPriority { get; set; } = false;

        public bool IsFeatured { get; set; } = false;

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string? RejectReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public int AreaId { get; set; }

        public Area? Area { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        public ICollection<PostImage> Images { get; set; } = new List<PostImage>();
    }
}