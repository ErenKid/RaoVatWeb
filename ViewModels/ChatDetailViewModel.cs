using RaoVatWeb.Models;

namespace RaoVatWeb.ViewModels
{
    public class ChatDetailViewModel
    {
        private string _partnerName = string.Empty;
        private string _otherUserName = string.Empty;

        public int ConversationId { get; set; }

        public int PostId { get; set; }

        public Post? Post { get; set; }

        public string PostTitle
        {
            get
            {
                if (Post != null && !string.IsNullOrWhiteSpace(Post.Title))
                {
                    return Post.Title;
                }

                return _postTitle;
            }
            set
            {
                _postTitle = value ?? string.Empty;
            }
        }

        private string _postTitle = string.Empty;

        public string CurrentUserId { get; set; } = string.Empty;

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

        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}