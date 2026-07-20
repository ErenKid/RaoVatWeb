using System.ComponentModel.DataAnnotations;

namespace RaoVatWeb.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int PostId { get; set; }

        public Post? Post { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(500)]
        public string Comment { get; set; } = string.Empty;

        public bool IsHidden { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}