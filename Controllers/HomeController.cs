using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models;
using RaoVatWeb.Models.Enums;
using RaoVatWeb.ViewModels;

namespace RaoVatWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? keyword, int? categoryId, int? areaId)
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var areas = await _context.Areas
                .Where(a => a.IsActive && a.ParentAreaId != null)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var query = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Area)
                .Include(p => p.Images)
                .Where(p => p.Status == PostStatus.Approved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(p =>
                    p.Title.Contains(keyword) ||
                    p.Description.Contains(keyword));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (areaId.HasValue && areaId.Value > 0)
            {
                query = query.Where(p => p.AreaId == areaId.Value);
            }

            var posts = await query
                .OrderByDescending(p => p.IsVipPriority)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();

            var model = new HomeIndexViewModel
            {
                Keyword = keyword,
                CategoryId = categoryId,
                AreaId = areaId,
                Categories = categories,
                Areas = areas,
                VipPosts = posts
                    .Where(p => p.IsVipPriority)
                    .Take(8)
                    .Select(MapPostToHomeItem)
                    .ToList(),
                LatestPosts = posts
                    .Take(16)
                    .Select(MapPostToHomeItem)
                    .ToList()
            };

            return View(model);
        }

        private static HomePostItemViewModel MapPostToHomeItem(Post post)
        {
            var imageUrl = post.Images?
                .OrderByDescending(i => i.IsMainImage)
                .ThenBy(i => i.PostImageId)
                .Select(i => i.ImageUrl)
                .FirstOrDefault();

            return new HomePostItemViewModel
            {
                PostId = post.PostId,
                Title = post.Title,
                Price = post.Price,
                CategoryName = post.Category?.Name ?? "Chưa có danh mục",
                AreaName = post.Area?.Name ?? "Chưa có khu vực",
                ImageUrl = imageUrl,
                CreatedAt = post.CreatedAt,
                IsVipPriority = post.IsVipPriority,
                ViewCount = post.ViewCount
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}