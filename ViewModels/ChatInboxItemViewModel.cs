namespace RaoVatWeb.ViewModels
{
    public class ChatInboxItemViewModel
    {
        private string _partnerName = string.Empty;
        private string _otherUserName = string.Empty;

        public int ConversationId { get; set; }

        public int PostId { get; set; }

        public string PostTitle { get; set; } = string.Empty;

        public string OtherUserId { get; set; } = string.Empty;

        public string PartnerName
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_partnerName)
                    ? _partnerName
                    : _otherUserName;
            }
            set
            {
                _partnerName = value ?? string.Empty;
            }
        }

        public string OtherUserName
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_otherUserName)
                    ? _otherUserName
                    : _partnerName;
            }
            set
            {
                _otherUserName = value ?? string.Empty;
            }
        }

        public string LastMessage { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
    }
}