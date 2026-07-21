using RaoVatWeb.Models;

namespace RaoVatWeb.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }

        public int TotalPosts { get; set; }

        public int PendingPosts { get; set; }

        public int ApprovedPosts { get; set; }

        public int RejectedPosts { get; set; }

        public int HiddenPosts { get; set; }

        public int TotalImages { get; set; }

        public int TotalCategories { get; set; }

        public int TotalAreas { get; set; }

        public int TotalVipOrders { get; set; }

        public int PaidVipOrders { get; set; }

        public decimal TotalVipRevenue { get; set; }

        public List<Post> RecentPosts { get; set; } = new List<Post>();
    }
}