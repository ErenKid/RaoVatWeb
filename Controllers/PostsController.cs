using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models;
using RaoVatWeb.ViewModels;
using RaoVatWeb.Models.Enums;
using Microsoft.AspNetCore.Identity;
namespace RaoVatWeb.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostsController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create()
{
    await LoadDropdownsAsync();

    var currentUser = await _userManager.GetUserAsync(User);

    ViewBag.ContactName = currentUser?.FullName ?? currentUser?.UserName ?? "";
    ViewBag.ContactPhone = currentUser?.PhoneNumber ?? "";

    return View(new PostCreateViewModel());
}
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(PostCreateViewModel model)
{
    if (!ModelState.IsValid)
    {
        await LoadDropdownsAsync();
        return View(model);
    }

    var currentUser = await _userManager.GetUserAsync(User);

    if (currentUser == null)
    {
        return Challenge();
    }

    var post = new Post
    {
        Title = model.Title.Trim(),
        Description = model.Description.Trim(),
        Price = model.Price,
        CategoryId = model.CategoryId,
        AreaId = model.AreaId,

        UserId = currentUser.Id,

        ContactName = currentUser.FullName,
        ContactPhone = currentUser.PhoneNumber ?? "",

        Status = PostStatus.Pending,
        IsVipPriority = false,
        ViewCount = 0,
        CreatedAt = DateTime.Now
    };

    _context.Posts.Add(post);
    await _context.SaveChangesAsync();

    TempData["Success"] = "Đăng tin thành công. Tin của bạn đang chờ Admin duyệt.";
    return RedirectToAction(nameof(MyPosts));
}

        public async Task<IActionResult> MyPosts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var posts = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Area)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        private async Task LoadDropdownsAsync()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var areas = await _context.Areas
                .Where(a => a.IsActive)
                .Where(a => a.ParentAreaId != null)
                .OrderBy(a => a.Name)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
            ViewBag.Areas = new SelectList(areas, "AreaId", "Name");
        }
    }
}