using System.ComponentModel.DataAnnotations;

namespace RaoVatWeb.Models
{
    public class Area
    {
        public int AreaId { get; set; }

        [Required(ErrorMessage = "Tên khu vực không được để trống")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ParentArea { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}