using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models;
using RaoVatWeb.ViewModels;

namespace RaoVatWeb.Controllers
{
    [Authorize]
    public class ChatsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            var conversations = await _context.Set<Conversation>()
                .Include(c => c.Post)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Where(c => c.BuyerId == currentUserId || c.SellerId == currentUserId)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();

            var model = conversations.Select(c =>
            {
                var otherUser = c.BuyerId == currentUserId
                    ? c.Seller
                    : c.Buyer;

                var otherUserId = c.BuyerId == currentUserId
                    ? c.SellerId
                    : c.BuyerId;

                var otherUserName = otherUser != null && !string.IsNullOrWhiteSpace(otherUser.FullName)
                    ? otherUser.FullName
                    : otherUser?.Email ?? "Người dùng";

                return new ChatInboxItemViewModel
                {
                    ConversationId = c.ConversationId,
                    PostId = c.PostId,
                    PostTitle = c.Post != null ? c.Post.Title : "Tin đã bị xóa",
                    OtherUserId = otherUserId,
                    OtherUserName = otherUserName,
                    PartnerName = otherUserName,
                    LastMessage = c.LastMessage ?? string.Empty,
                    UpdatedAt = c.UpdatedAt
                };
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            var conversation = await _context.Set<Conversation>()
                .Include(c => c.Post)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.ConversationId == id);

            if (conversation == null)
            {
                TempData["Error"] = "Không tìm thấy cuộc trò chuyện.";
                return RedirectToAction(nameof(Index));
            }

            if (conversation.BuyerId != currentUserId && conversation.SellerId != currentUserId)
            {
                return Forbid();
            }

            var otherUser = conversation.BuyerId == currentUserId
                ? conversation.Seller
                : conversation.Buyer;

            var otherUserId = conversation.BuyerId == currentUserId
                ? conversation.SellerId
                : conversation.BuyerId;

            var otherUserName = otherUser != null && !string.IsNullOrWhiteSpace(otherUser.FullName)
                ? otherUser.FullName
                : otherUser?.Email ?? "Người dùng";

            var model = new ChatDetailViewModel
            {
                ConversationId = conversation.ConversationId,
                PostId = conversation.PostId,
                Post = conversation.Post,
                PostTitle = conversation.Post != null ? conversation.Post.Title : "Tin đã bị xóa",
                CurrentUserId = currentUserId,
                OtherUserId = otherUserId,
                OtherUserName = otherUserName,
                PartnerName = otherUserName,
                Messages = conversation.Messages
                    .OrderBy(m => m.CreatedAt)
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int id, string messageContent)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(messageContent))
            {
                TempData["Error"] = "Vui lòng nhập nội dung tin nhắn.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var conversation = await _context.Set<Conversation>()
                .FirstOrDefaultAsync(c => c.ConversationId == id);

            if (conversation == null)
            {
                TempData["Error"] = "Không tìm thấy cuộc trò chuyện.";
                return RedirectToAction(nameof(Index));
            }

            if (conversation.BuyerId != currentUserId && conversation.SellerId != currentUserId)
            {
                return Forbid();
            }

            var content = messageContent.Trim();

            var message = new ChatMessage
{
    ConversationId = conversation.ConversationId,
    SenderId = currentUserId,
    Content = content,
    IsRead = false,
    CreatedAt = DateTime.Now
};

            _context.Set<ChatMessage>().Add(message);

            conversation.LastMessage = content;
            conversation.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = conversation.ConversationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int postId)
        {
            return await StartChat(postId);
        }

        [HttpGet]
        public async Task<IActionResult> Start(int postId, bool fromLink = true)
        {
            return await StartChat(postId);
        }

        private async Task<IActionResult> StartChat(int postId)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null)
            {
                TempData["Error"] = "Không tìm thấy tin đăng.";
                return RedirectToAction("Index", "Posts");
            }

            if (post.UserId == currentUserId)
            {
                TempData["Error"] = "Bạn không thể tự nhắn tin cho tin đăng của mình.";
                return RedirectToAction("Details", "Posts", new { id = postId });
            }

            var sellerId = post.UserId;
            var buyerId = currentUserId;

            var conversation = await _context.Set<Conversation>()
                .FirstOrDefaultAsync(c =>
                    c.PostId == postId &&
                    c.BuyerId == buyerId &&
                    c.SellerId == sellerId);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    PostId = postId,
                    BuyerId = buyerId,
                    SellerId = sellerId,
                    LastMessage = string.Empty,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Set<Conversation>().Add(conversation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = conversation.ConversationId });
        }
    }
}