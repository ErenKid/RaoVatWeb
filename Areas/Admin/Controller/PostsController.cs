using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace RaoVatWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private async Task LoadDropdownsAsync(int? selectedCategoryId = null, int? selectedAreaId = null)
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var areas = await _context.Areas
                .Where(a => a.IsActive)
                .Where(a => a.ParentAreaId != null)
                .OrderBy(a => a.Name)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", selectedCategoryId);
            ViewBag.Areas = new SelectList(areas, "AreaId", "Name", selectedAreaId);
        }
        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? keyword, int? categoryId, int? areaId)
        {
            await LoadDropdownsAsync(categoryId, areaId);

            var query = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Area)
                .Where(p => p.Status == PostStatus.Approved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p =>
                    p.Title.Contains(keyword) ||
                    p.Description.Contains(keyword));
            }

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                if (areaId.HasValue)
                {
                    query = query.Where(p => p.AreaId == areaId.Value);
                }

                var posts = await query
                    .OrderByDescending(p => p.IsVipPriority)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToListAsync();

                ViewBag.Keyword = keyword;
                ViewBag.CategoryId = categoryId;
                ViewBag.AreaId = areaId;

                return View(posts);
            }

            [AllowAnonymous]
            public async Task<IActionResult> Details(int id)
            {
                var post = await _context.Posts
                    .Include(p => p.Category)
                    .Include(p => p.Area)
                    .FirstOrDefaultAsync(p =>
                        p.PostId == id &&
                        p.Status == PostStatus.Approved);

                if (post == null)
                {
                    return NotFound();
                }

                post.ViewCount += 1;
                await _context.SaveChangesAsync();

                return View(post);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            post.Status = PostStatus.Approved;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã duyệt tin đăng thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            post.Status = PostStatus.Rejected;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã từ chối tin đăng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            post.Status = PostStatus.Hidden;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã ẩn tin đăng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreToPending(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            post.Status = PostStatus.Pending;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã chuyển tin về trạng thái chờ duyệt.";
            return RedirectToAction(nameof(Index));
        }
        
    }
}