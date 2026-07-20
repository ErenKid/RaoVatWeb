using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models;
using RaoVatWeb.Models.Enums;
using System.Security.Claims;

namespace RaoVatWeb.Controllers
{
    [Authorize]
    public class ReviewReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int reviewId, string reason)
        {
            var reporterId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(reporterId))
            {
                return Challenge();
            }

            var review = await _context.Reviews
                .Include(r => r.Post)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && !r.IsHidden);

            if (review == null)
            {
                TempData["Error"] = "Đánh giá không tồn tại hoặc đã bị ẩn.";
                return RedirectToAction("Index", "Posts");
            }

            if (review.UserId == reporterId)
            {
                TempData["Error"] = "Bạn không thể báo cáo đánh giá của chính mình.";
                return RedirectToAction("Details", "Posts", new { id = review.PostId });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Vui lòng nhập lý do báo cáo.";
                return RedirectToAction("Details", "Posts", new { id = review.PostId });
            }

            var existedReport = await _context.ReviewReports
                .FirstOrDefaultAsync(r =>
                    r.ReviewId == reviewId &&
                    r.ReporterId == reporterId &&
                    r.Status == ReviewReportStatus.Pending);

            if (existedReport != null)
            {
                existedReport.Reason = reason.Trim();
                existedReport.CreatedAt = DateTime.Now;

                _context.ReviewReports.Update(existedReport);
            }
            else
            {
                var report = new ReviewReport
                {
                    ReviewId = reviewId,
                    ReporterId = reporterId,
                    Reason = reason.Trim(),
                    Status = ReviewReportStatus.Pending,
                    CreatedAt = DateTime.Now
                };

                _context.ReviewReports.Add(report);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Báo cáo đánh giá đã được gửi đến Admin.";
            return RedirectToAction("Details", "Posts", new { id = review.PostId });
        }
    }
}