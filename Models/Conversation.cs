using System.ComponentModel.DataAnnotations;

namespace RaoVatWeb.Models
{
    public class Conversation
    {
        public int ConversationId { get; set; }

        public int PostId { get; set; }

        public Post? Post { get; set; }

        [Required]
        public string BuyerId { get; set; } = string.Empty;

        public ApplicationUser? Buyer { get; set; }

        [Required]
        public string SellerId { get; set; } = string.Empty;

        public ApplicationUser? Seller { get; set; }

        public string? LastMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}