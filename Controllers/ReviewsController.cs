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
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int postId, int rating, string comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(p =>
                    p.PostId == postId &&
                    p.Status == PostStatus.Approved);

            if (post == null)
            {
                TempData["Error"] = "Tin đăng không tồn tại hoặc chưa được duyệt.";
                return RedirectToAction("Index", "Posts");
            }

            if (post.UserId == userId)
            {
                TempData["Error"] = "Bạn không thể tự đánh giá tin đăng của mình.";
                return RedirectToAction("Details", "Posts", new { id = postId });
            }

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Số sao đánh giá không hợp lệ.";
                return RedirectToAction("Details", "Posts", new { id = postId });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Vui lòng nhập nội dung đánh giá.";
                return RedirectToAction("Details", "Posts", new { id = postId });
            }

            var existedReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);

            if (existedReview != null)
            {
                existedReview.Rating = rating;
                existedReview.Comment = comment.Trim();
                existedReview.IsHidden = false;
                existedReview.CreatedAt = DateTime.Now;

                _context.Reviews.Update(existedReview);
            }
            else
            {
                var review = new Review
                {
                    PostId = postId,
                    UserId = userId,
                    Rating = rating,
                    Comment = comment.Trim(),
                    IsHidden = false,
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(review);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đánh giá của bạn đã được ghi nhận.";
            return RedirectToAction("Details", "Posts", new { id = postId });
        }
    }
}