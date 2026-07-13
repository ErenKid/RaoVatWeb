using RaoVatWeb.Models;

namespace RaoVatWeb.ViewModels
{
    public class ChatDetailViewModel
    {
        public int ConversationId { get; set; }

        public string CurrentUserId { get; set; } = string.Empty;

        public string PartnerName { get; set; } = string.Empty;

        public string PostTitle { get; set; } = string.Empty;

        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}