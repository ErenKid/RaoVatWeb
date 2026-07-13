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
        private async Task LoadDropdownsAsync(int? selectedCategoryId = null, int? selectedAreaId = null)
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
                 ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", selectedCategoryId);
                ViewBag.Areas = new SelectList(areas, "AreaId", "Name", selectedAreaId);
            }
        public async Task<IActionResult> Edit(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == id && p.UserId == currentUser.Id);

            if (post == null)
            {
                return NotFound();
            }

            await LoadDropdownsAsync(post.CategoryId, post.AreaId);
    
            var model = new PostEditViewModel
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                Price = Convert.ToDecimal(post.Price),
                CategoryId = post.CategoryId,
                AreaId = post.AreaId
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostEditViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model.CategoryId, model.AreaId);
                return View(model);
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == model.PostId && p.UserId == currentUser.Id);

            if (post == null)
            {
                return NotFound();
            }

            post.Title = model.Title.Trim();
            post.Description = model.Description.Trim();
            post.Price = model.Price;
            post.CategoryId = model.CategoryId;
            post.AreaId = model.AreaId;

            post.ContactName = currentUser.FullName ?? currentUser.UserName ?? "";
            post.ContactPhone = currentUser.PhoneNumber ?? "";

            post.Status = PostStatus.Pending;
            post.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật tin thành công. Tin đã được chuyển về trạng thái chờ duyệt.";
            return RedirectToAction(nameof(MyPosts));
        }
        public PostsController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
         [AllowAnonymous]
        public async Task<IActionResult> Index(string? keyword, int? categoryId, int? areaId)
        {
            await LoadDropdownsAsync(categoryId, areaId);

            var query = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Area)
                .Where(p => p.Status == PostStatus.Approved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p =>
                    p.Title.Contains(keyword) ||
                    p.Description.Contains(keyword));
            }

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                if (areaId.HasValue)
                {
                    query = query.Where(p => p.AreaId == areaId.Value);
                }

                var posts = await query
                    .OrderByDescending(p => p.IsVipPriority)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToListAsync();

                ViewBag.Keyword = keyword;
                ViewBag.CategoryId = categoryId;
                ViewBag.AreaId = areaId;

                return View(posts);
            }
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Hide(int id)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null)
                {
                    return Challenge();
                }

                var post = await _context.Posts
                    .FirstOrDefaultAsync(p => p.PostId == id && p.UserId == currentUser.Id);

                if (post == null)
                {
                    return NotFound();
                }

                post.Status = PostStatus.Hidden;
                post.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã ẩn tin thành công.";
                return RedirectToAction(nameof(MyPosts));
            }
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Delete(int id)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null)
                {
                    return Challenge();
                }

                var post = await _context.Posts
                    .FirstOrDefaultAsync(p => p.PostId == id && p.UserId == currentUser.Id);

                if (post == null)
                {
                    return NotFound();
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã xóa tin thành công.";
                return RedirectToAction(nameof(MyPosts));
            }
            [AllowAnonymous]
            public async Task<IActionResult> Details(int id)
            {
                var post = await _context.Posts
                    .Include(p => p.Category)
                    .Include(p => p.Area)
                    .FirstOrDefaultAsync(p =>
                        p.PostId == id &&
                        p.Status == PostStatus.Approved);

                if (post == null)
                {
                    return NotFound();
                }

                post.ViewCount += 1;
                await _context.SaveChangesAsync();

                return View(post);
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
    }
}