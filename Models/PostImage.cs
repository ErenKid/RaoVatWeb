using System.ComponentModel.DataAnnotations;

namespace RaoVatWeb.Models
{
    public class PostImage
    {
        public int PostImageId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMainImage { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int PostId { get; set; }

        public Post? Post { get; set; }
    }
}