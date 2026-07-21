using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models;
using RaoVatWeb.Models.Enums;
using System.Security.Claims;

namespace RaoVatWeb.Controllers
{
    public class VipPackagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VipPackagesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var packages = await _context.VipPackages
                .Where(x => x.IsActive)
                .OrderBy(x => x.Price)
                .ToListAsync();

            return View(packages);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vipPackage = await _context.VipPackages
                .FirstOrDefaultAsync(x => x.VipPackageId == id && x.IsActive);

            if (vipPackage == null)
            {
                TempData["Error"] = "Gói VIP không tồn tại hoặc đã bị tắt.";
                return RedirectToAction(nameof(Index));
            }

            var order = new VipOrder
            {
                UserId = userId,
                VipPackageId = vipPackage.VipPackageId,
                VipPackage = vipPackage,
                Amount = vipPackage.Price,
                Status = VipOrderStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.VipOrders.Add(order);
            await _context.SaveChangesAsync();

            order = await _context.VipOrders
                .Include(x => x.VipPackage)
                .FirstAsync(x => x.VipOrderId == order.VipOrderId);

            return View(order);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var order = await _context.VipOrders
                .Include(x => x.VipPackage)
                .FirstOrDefaultAsync(x =>
                    x.VipOrderId == id &&
                    x.UserId == userId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn VIP.";
                return RedirectToAction(nameof(Index));
            }

            if (order.Status == VipOrderStatus.Paid)
            {
                return RedirectToAction(nameof(Success), new { id = order.VipOrderId });
            }

            if (order.VipPackage == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin gói VIP.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.FindByIdAsync(userId);

            if (currentUser == null)
            {
                return Challenge();
            }

            var now = DateTime.Now;

            var startDate = currentUser.VipExpiredAt.HasValue &&
                            currentUser.VipExpiredAt.Value > now
                ? currentUser.VipExpiredAt.Value
                : now;

            var newExpiredAt = startDate.AddDays(order.VipPackage.DurationDays);

            order.Status = VipOrderStatus.Paid;
            order.PaidAt = now;
            order.VipExpiredAt = newExpiredAt;

            currentUser.IsVip = true;
            currentUser.VipExpiredAt = newExpiredAt;

            await _userManager.UpdateAsync(currentUser);

            var userPosts = await _context.Posts
                .Where(p => p.UserId == userId
                            && p.Status != PostStatus.Rejected
                            && p.Status != PostStatus.Hidden)
                .ToListAsync();

            foreach (var post in userPosts)
            {
                post.IsVipPriority = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Success), new { id = order.VipOrderId });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.VipOrders
                .Include(x => x.VipPackage)
                .FirstOrDefaultAsync(x =>
                    x.VipOrderId == id &&
                    x.UserId == userId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn VIP.";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.VipOrders
                .Include(x => x.VipPackage)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(orders);
        }
    }
}

