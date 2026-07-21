namespace RaoVatWeb.ViewModels
{
    public class HomePostItemViewModel
    {
        public int PostId { get; set; }

        public string Title { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string AreaName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsVipPriority { get; set; }

        public int ViewCount { get; set; }
    }
}