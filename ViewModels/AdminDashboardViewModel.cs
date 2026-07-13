using RaoVatWeb.Models;

namespace RaoVatWeb.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalPosts { get; set; }

        public int PendingPosts { get; set; }

        public int ApprovedPosts { get; set; }

        public int RejectedPosts { get; set; }

        public int HiddenPosts { get; set; }

        public int TotalUsers { get; set; }

        public int TotalImages { get; set; }

        public List<Post> RecentPosts { get; set; } = new List<Post>();
    }
}