using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models;
using RaoVatWeb.Models.Enums;

namespace RaoVatWeb.Controllers
{
    [Authorize]
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

        public async Task<IActionResult> Index()
        {
            var packages = await _context.VipPackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToListAsync();

            var currentUser = await _userManager.GetUserAsync(User);

            ViewBag.IsVipActive = currentUser != null &&
                                  currentUser.IsVip &&
                                  currentUser.VipExpiredAt.HasValue &&
                                  currentUser.VipExpiredAt.Value > DateTime.Now;

            ViewBag.VipExpiredAt = currentUser?.VipExpiredAt;

            return View(packages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int packageId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var package = await _context.VipPackages
                .FirstOrDefaultAsync(p => p.VipPackageId == packageId && p.IsActive);

            if (package == null)
            {
                return NotFound();
            }

            var orderCode = $"VIP{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

            var order = new VipOrder
            {
                OrderCode = orderCode,
                UserId = currentUser.Id,
                VipPackageId = package.VipPackageId,
                Amount = package.Price,
                Status = VipOrderStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.VipOrders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Checkout), new { id = order.VipOrderId });
        }

        public async Task<IActionResult> Checkout(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var order = await _context.VipOrders
                .Include(o => o.VipPackage)
                .FirstOrDefaultAsync(o =>
                    o.VipOrderId == id &&
                    o.UserId == currentUser.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == VipOrderStatus.Paid)
            {
                return RedirectToAction(nameof(Success), new { id = order.VipOrderId });
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var order = await _context.VipOrders
                .Include(o => o.VipPackage)
                .FirstOrDefaultAsync(o =>
                    o.VipOrderId == id &&
                    o.UserId == currentUser.Id &&
                    o.Status == VipOrderStatus.Pending);

            if (order == null)
            {
                return NotFound();
            }

            if (order.VipPackage == null)
            {
                return NotFound();
            }

            var now = DateTime.Now;

            var startDate = currentUser.IsVip &&
                            currentUser.VipExpiredAt.HasValue &&
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
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Success), new { id = order.VipOrderId });
        }

        public async Task<IActionResult> Success(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var order = await _context.VipOrders
                .Include(o => o.VipPackage)
                .FirstOrDefaultAsync(o =>
                    o.VipOrderId == id &&
                    o.UserId == currentUser.Id &&
                    o.Status == VipOrderStatus.Paid);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var order = await _context.VipOrders
                .FirstOrDefaultAsync(o =>
                    o.VipOrderId == id &&
                    o.UserId == currentUser.Id &&
                    o.Status == VipOrderStatus.Pending);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = VipOrderStatus.Cancelled;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã hủy đơn đăng ký VIP.";
            return RedirectToAction(nameof(Index));
        }
    }
}