using System.ComponentModel.DataAnnotations;

namespace RaoVatWeb.Models
{
    public class ContactMessage
    {
        public int ContactMessageId { get; set; }

        public int PostId { get; set; }

        public Post? Post { get; set; }

        [Required]
        [StringLength(100)]
        public string SenderName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string SenderPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SenderEmail { get; set; }

        [Required]
        public string MessageContent { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}