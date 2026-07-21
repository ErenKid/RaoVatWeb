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
                TotalUsers = await _userManager.Users.CountAsync(),

                TotalPosts = await _context.Posts.CountAsync(),

                PendingPosts = await _context.Posts
                    .CountAsync(p => p.Status == PostStatus.Pending),

                ApprovedPosts = await _context.Posts
                    .CountAsync(p => p.Status == PostStatus.Approved),

                RejectedPosts = await _context.Posts
                    .CountAsync(p => p.Status == PostStatus.Rejected),

                HiddenPosts = await _context.Posts
                    .CountAsync(p => p.Status == PostStatus.Hidden),

                TotalImages = await _context.PostImages.CountAsync(),

                TotalCategories = await _context.Categories.CountAsync(),

                TotalAreas = await _context.Areas.CountAsync(),

                TotalVipOrders = await _context.VipOrders.CountAsync(),

                PaidVipOrders = await _context.VipOrders
                    .CountAsync(x => x.Status == VipOrderStatus.Paid),

                TotalVipRevenue = await _context.VipOrders
                    .Where(x => x.Status == VipOrderStatus.Paid)
                    .SumAsync(x => x.Amount),

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