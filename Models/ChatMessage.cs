using System.ComponentModel.DataAnnotations;

namespace RaoVatWeb.Models
{
    public class ChatMessage
    {
        public int ChatMessageId { get; set; }

        public int ConversationId { get; set; }

        public Conversation? Conversation { get; set; }

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}