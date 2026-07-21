using RaoVatWeb.Models;

namespace RaoVatWeb.ViewModels
{
    public class HomeIndexViewModel
    {
        public string? Keyword { get; set; }

        public int? CategoryId { get; set; }

        public int? AreaId { get; set; }

        public List<Category> Categories { get; set; } = new();

        public List<Area> Areas { get; set; } = new();

        public List<HomePostItemViewModel> VipPosts { get; set; } = new();

        public List<HomePostItemViewModel> LatestPosts { get; set; } = new();
    }
}