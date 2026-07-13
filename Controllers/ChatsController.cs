using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models;
using RaoVatWeb.Models.Enums;
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
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var conversations = await _context.Conversations
                .Include(c => c.Post)
                .Include(c => c.Messages)
                .Where(c => c.BuyerId == currentUser.Id || c.SellerId == currentUser.Id)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();

            var model = new List<ChatInboxItemViewModel>();

            foreach (var conversation in conversations)
            {
                var partnerId = conversation.BuyerId == currentUser.Id
                    ? conversation.SellerId
                    : conversation.BuyerId;

                var partner = await _userManager.FindByIdAsync(partnerId);

                var lastMessage = conversation.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();

                model.Add(new ChatInboxItemViewModel
                {
                    ConversationId = conversation.ConversationId,
                    PostTitle = conversation.Post != null ? conversation.Post.Title : "Tin đã bị xóa",
                    PartnerName = partner?.FullName ?? partner?.UserName ?? "Người dùng",
                    LastMessage = lastMessage != null ? lastMessage.Content : "Chưa có tin nhắn",
                    UpdatedAt = conversation.UpdatedAt
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int postId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(p =>
                    p.PostId == postId &&
                    p.Status == PostStatus.Approved);

            if (post == null)
            {
                return NotFound();
            }

            if (post.UserId == currentUser.Id)
            {
                TempData["Error"] = "Bạn không thể nhắn tin với chính tin của mình.";
                return RedirectToAction("Details", "Posts", new { id = postId });
            }

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.PostId == postId &&
                    c.BuyerId == currentUser.Id &&
                    c.SellerId == post.UserId);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    PostId = post.PostId,
                    BuyerId = currentUser.Id,
                    SellerId = post.UserId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = conversation.ConversationId });
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var conversation = await _context.Conversations
                .Include(c => c.Post)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c =>
                    c.ConversationId == id &&
                    (c.BuyerId == currentUser.Id || c.SellerId == currentUser.Id));

            if (conversation == null)
            {
                return NotFound();
            }

            var partnerId = conversation.BuyerId == currentUser.Id
                ? conversation.SellerId
                : conversation.BuyerId;

            var partner = await _userManager.FindByIdAsync(partnerId);

            var unreadMessages = conversation.Messages
                .Where(m => m.SenderId != currentUser.Id && !m.IsRead)
                .ToList();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();

            var model = new ChatDetailViewModel
            {
                ConversationId = conversation.ConversationId,
                CurrentUserId = currentUser.Id,
                PartnerName = partner?.FullName ?? partner?.UserName ?? "Người dùng",
                PostTitle = conversation.Post != null ? conversation.Post.Title : "Tin đã bị xóa",
                Messages = conversation.Messages
                    .OrderBy(m => m.CreatedAt)
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int conversationId, string content)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.ConversationId == conversationId &&
                    (c.BuyerId == currentUser.Id || c.SellerId == currentUser.Id));

            if (conversation == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Vui lòng nhập nội dung tin nhắn.";
                return RedirectToAction(nameof(Details), new { id = conversationId });
            }

            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = currentUser.Id,
                Content = content.Trim(),
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            conversation.UpdatedAt = DateTime.Now;

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = conversationId });
        }
    }
}