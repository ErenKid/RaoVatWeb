using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models;
using RaoVatWeb.Models.Enums;
using RaoVatWeb.ViewModels;

namespace RaoVatWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalPosts = await _context.Posts.CountAsync(),

                PendingPosts = await _context.Posts
                    .CountAsync(p => p.Status == PostStatus.Pending),

                ApprovedPosts = await _context.Posts
                    .CountAsync(p => p.Status == PostStatus.Approved),

                RejectedPosts = await _context.Posts
                    .CountAsync(p => p.Status == PostStatus.Rejected),

                HiddenPosts = await _context.Posts
                    .CountAsync(p => p.Status == PostStatus.Hidden),

                TotalUsers = await _userManager.Users.CountAsync(),

                TotalImages = await _context.PostImages.CountAsync(),

                RecentPosts = await _context.Posts
                    .Include(p => p.Category)
                    .Include(p => p.Area)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}