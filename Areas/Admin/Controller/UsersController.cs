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
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var model = new List<AdminUserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var postCount = await _context.Posts
                    .CountAsync(p => p.UserId == user.Id);

                var isLocked = user.LockoutEnd.HasValue &&
                               user.LockoutEnd.Value > DateTimeOffset.UtcNow;

                model.Add(new AdminUserViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    FullName = user.FullName ?? user.UserName ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Roles = roles.Any() ? string.Join(", ", roles) : "User",
                    PostCount = postCount,
                    IsLocked = isLocked,
                    LockoutEnd = user.LockoutEnd
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            if (currentUser.Id == id)
            {
                TempData["Error"] = "Bạn không thể khóa chính tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Không nên khóa tài khoản Admin.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

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
                return NotFound();
            }

            await _userManager.SetLockoutEndDateAsync(user, null);

            TempData["Success"] = "Đã mở khóa tài khoản người dùng.";
            return RedirectToAction(nameof(Index));
        }
    }
}