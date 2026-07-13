namespace RaoVatWeb.ViewModels
{
    public class ChatInboxItemViewModel
    {
        public int ConversationId { get; set; }

        public string PostTitle { get; set; } = string.Empty;

        public string PartnerName { get; set; } = string.Empty;

        public string LastMessage { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
    }
}