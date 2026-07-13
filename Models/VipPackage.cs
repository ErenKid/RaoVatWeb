using System.ComponentModel.DataAnnotations;

namespace RaoVatWeb.Models
{
    public class VipPackage
    {
        public int VipPackageId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int DurationDays { get; set; }

        public int MaxPosts { get; set; }

        public bool CanHighlightPost { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<VipOrder> VipOrders { get; set; } = new List<VipOrder>();
    }
}