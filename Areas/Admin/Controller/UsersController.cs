using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models;
using RaoVatWeb.ViewModels;

namespace RaoVatWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var users = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var model = new List<AdminUserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var postCount = await _context.Posts
                    .CountAsync(p => p.UserId == user.Id);

                model.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,

                    CreatedAt = user.CreatedAt,

                    IsVip = user.IsVip
                            && user.VipExpiredAt.HasValue
                            && user.VipExpiredAt.Value > now,

                    VipExpiredAt = user.VipExpiredAt,

                    IsLocked = user.LockoutEnd.HasValue
                               && user.LockoutEnd.Value > DateTimeOffset.UtcNow,

                    LockoutEnd = user.LockoutEnd,

                    Roles = string.Join(", ", roles),

                    PostCount = postCount
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (id == currentUserId)
            {
                TempData["Error"] = "Bạn không thể tự khóa tài khoản Admin đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Index));
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Đã khóa tài khoản người dùng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Index));
            }

            user.LockoutEnd = null;
            user.AccessFailedCount = 0;

            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Đã mở khóa tài khoản người dùng.";
            return RedirectToAction(nameof(Index));
        }
    }
}