using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RaoVatWeb.Models.Enums;
namespace RaoVatWeb.Models
{
    public class Post
    {
        public int PostId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề tin đăng")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả tin đăng")]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khu vực")]
        public int AreaId { get; set; }

        public Area? Area { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người liên hệ")]
        [StringLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ")]
        [StringLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        public PostStatus Status { get; set; } = PostStatus.Pending;

        public bool IsActive { get; set; } = true;

        public bool IsVipPriority { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
        public ICollection<PostImage> Images { get; set; } = new List<PostImage>();
        public ICollection<ContactMessage> ContactMessages { get; set; } = new List<ContactMessage>();
        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    }
}