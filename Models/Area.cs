using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaoVatWeb.Models
{
    public class Area
    {
        public int AreaId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int? ParentAreaId { get; set; }

        [ForeignKey("ParentAreaId")]
        public Area? ParentArea { get; set; }

        public ICollection<Area> Children { get; set; } = new List<Area>();

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}