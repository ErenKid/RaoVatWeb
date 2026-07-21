using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RaoVatWeb.Models;
using RaoVatWeb.ViewModels;

namespace RaoVatWeb.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var model = BuildModel(user);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(AccountSettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                TempData["Error"] = "Vui lòng nhập họ tên.";
                return RedirectToAction(nameof(Index));
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Đã cập nhật thông tin tài khoản.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(AccountSettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                TempData["Error"] = "Vui lòng nhập mật khẩu hiện tại.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                TempData["Error"] = "Vui lòng nhập mật khẩu mới.";
                return RedirectToAction(nameof(Index));
            }

            if (model.NewPassword.Length < 6)
            {
                TempData["Error"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return RedirectToAction(nameof(Index));
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["Error"] = "Xác nhận mật khẩu mới không khớp.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword
            );

            if (!result.Succeeded)
            {
                TempData["Error"] = "Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu hiện tại.";
                return RedirectToAction(nameof(Index));
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Đã đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Index));
        }

        private static AccountSettingsViewModel BuildModel(ApplicationUser user)
        {
            var now = DateTime.Now;

            return new AccountSettingsViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,

                IsVip = user.IsVip
                        && user.VipExpiredAt.HasValue
                        && user.VipExpiredAt.Value > now,

                VipExpiredAt = user.VipExpiredAt
            };
        }
    }
}