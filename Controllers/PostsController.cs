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
                public PostsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
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
                    .Include(p => p.Images)
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
                    AreaId = post.AreaId,
                    ExistingImages = post.Images.ToList()
                };

                return View(model);
            }

            [AllowAnonymous]
public async Task<IActionResult> Index(string? keyword, int? categoryId, int? areaId)
{
    await LoadDropdownsAsync(categoryId, areaId);

    var query = _context.Posts
        .Include(p => p.Category)
        .Include(p => p.Area)
        .Include(p => p.Images)
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

                    model.ExistingImages = await _context.PostImages
                        .Where(x => x.PostId == model.PostId)
                        .ToListAsync();

                    return View(model);
                }

                var post = await _context.Posts
                    .Include(p => p.Images)
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

                var files = Request.Form.Files;

                if (files != null && files.Count > 0)
                {
                    var uploadFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "posts"
                    );

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var maxFileSize = 5 * 1024 * 1024;

                    var hasMainImage = await _context.PostImages
                        .AnyAsync(x => x.PostId == post.PostId && x.IsMainImage);

                    for (int i = 0; i < files.Count; i++)
                    {
                        var image = files[i];

                        if (image == null || image.Length == 0)
                        {
                            continue;
                        }

                        if (image.Length > maxFileSize)
                        {
                            ModelState.AddModelError("Images", "Mỗi ảnh không được vượt quá 5MB.");
                            await LoadDropdownsAsync(model.CategoryId, model.AreaId);

                            model.ExistingImages = await _context.PostImages
                                .Where(x => x.PostId == model.PostId)
                                .ToListAsync();

                            return View(model);
                        }

                        var extension = Path.GetExtension(image.FileName).ToLower();

                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("Images", "Chỉ chấp nhận ảnh JPG, JPEG hoặc PNG.");
                            await LoadDropdownsAsync(model.CategoryId, model.AreaId);

                            model.ExistingImages = await _context.PostImages
                                .Where(x => x.PostId == model.PostId)
                                .ToListAsync();

                            return View(model);
                        }

                        var fileName = $"{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(uploadFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var postImage = new PostImage
                        {
                            PostId = post.PostId,
                            ImageUrl = $"/uploads/posts/{fileName}",
                            IsMainImage = !hasMainImage && i == 0,
                            CreatedAt = DateTime.Now
                        };

                        _context.PostImages.Add(postImage);
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Cập nhật tin thành công. Tin đã được chuyển về trạng thái chờ duyệt.";
                return RedirectToAction(nameof(MyPosts));
            }

            [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteImage(int imageId, int postId)
{
    var currentUser = await _userManager.GetUserAsync(User);

    if (currentUser == null)
    {
        return Challenge();
    }

    var image = await _context.PostImages
        .Include(x => x.Post)
        .FirstOrDefaultAsync(x =>
            x.PostImageId == imageId &&
            x.PostId == postId &&
            x.Post != null &&
            x.Post.UserId == currentUser.Id);

    if (image == null)
    {
        return NotFound();
    }

    var post = image.Post;

    var imagePath = image.ImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);

    if (System.IO.File.Exists(fullPath))
    {
        System.IO.File.Delete(fullPath);
    }

    var wasMainImage = image.IsMainImage;

    _context.PostImages.Remove(image);

    post.Status = PostStatus.Pending;
    post.UpdatedAt = DateTime.Now;

    await _context.SaveChangesAsync();

    if (wasMainImage)
    {
        var nextImage = await _context.PostImages
            .Where(x => x.PostId == postId)
            .OrderBy(x => x.PostImageId)
            .FirstOrDefaultAsync();

        if (nextImage != null)
        {
            nextImage.IsMainImage = true;
            await _context.SaveChangesAsync();
        }
    }

    TempData["Success"] = "Đã xóa hình ảnh. Tin đã được chuyển về trạng thái chờ duyệt.";
    return RedirectToAction(nameof(Edit), new { id = postId });
}
            [AllowAnonymous]
public async Task<IActionResult> Details(int id)
{
    var post = await _context.Posts
        .Include(p => p.Category)
        .Include(p => p.Area)
        .Include(p => p.Images)
        .FirstOrDefaultAsync(p =>
            p.PostId == id &&
            p.Status == PostStatus.Approved);

    if (post == null)
    {
        return NotFound();
    }

    post.ViewCount += 1;
    await _context.SaveChangesAsync();

    var reviews = await _context.Reviews
        .Include(r => r.User)
        .Where(r => r.PostId == id && !r.IsHidden)
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();

    ViewBag.Reviews = reviews;
    ViewBag.ReviewCount = reviews.Count;
    ViewBag.AverageRating = reviews.Any()
        ? reviews.Average(r => r.Rating)
        : 0;

    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    ViewBag.HasReviewed = !string.IsNullOrEmpty(currentUserId)
                          && reviews.Any(r => r.UserId == currentUserId);

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
        IsVipPriority = currentUser.IsVip &&
                currentUser.VipExpiredAt.HasValue &&
                currentUser.VipExpiredAt.Value > DateTime.Now,
        ViewCount = 0,
        CreatedAt = DateTime.Now
    };

    _context.Posts.Add(post);
    await _context.SaveChangesAsync();
            if (model.Images != null && model.Images.Any())
        {
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "posts");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var maxFileSize = 5 * 1024 * 1024; // 5MB

            for (int i = 0; i < model.Images.Count; i++)
            {
                var image = model.Images[i];

                if (image.Length <= 0)
                {
                    continue;
                }

                if (image.Length > maxFileSize)
                {
                    ModelState.AddModelError("Images", "Mỗi ảnh không được vượt quá 5MB.");
                    await LoadDropdownsAsync();
                    return View(model);
                }

                var extension = Path.GetExtension(image.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Images", "Chỉ chấp nhận ảnh JPG, JPEG hoặc PNG.");
                    await LoadDropdownsAsync();
                    return View(model);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var postImage = new PostImage
                {
                    PostId = post.PostId,
                    ImageUrl = $"/uploads/posts/{fileName}",
                    IsMainImage = i == 0,
                    CreatedAt = DateTime.Now
                };

                _context.PostImages.Add(postImage);
            }

            await _context.SaveChangesAsync();
        }

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