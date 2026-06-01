using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models.Enums;

namespace RaoVatWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(PostStatus? status)
        {
            var query = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Area)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.CurrentStatus = status;

            ViewBag.PendingCount = await _context.Posts.CountAsync(p => p.Status == PostStatus.Pending);
            ViewBag.ApprovedCount = await _context.Posts.CountAsync(p => p.Status == PostStatus.Approved);
            ViewBag.RejectedCount = await _context.Posts.CountAsync(p => p.Status == PostStatus.Rejected);
            ViewBag.HiddenCount = await _context.Posts.CountAsync(p => p.Status == PostStatus.Hidden);

            return View(posts);
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