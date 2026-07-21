using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models;

namespace RaoVatWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Posts)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Posts)
                    .ThenInclude(p => p.Area)
                .Include(c => c.Posts)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var category = new Category
            {
                IsActive = true
            };

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,IsActive")] Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var existed = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());

            if (existed)
            {
                ModelState.AddModelError("Name", "Tên danh mục đã tồn tại.");
                return View(category);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã thêm danh mục mới.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,IsActive")] Category category)
        {
            if (id != category.CategoryId)
            {
                TempData["Error"] = "Dữ liệu danh mục không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var currentCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (currentCategory == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(Index));
            }

            var existed = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryId != id &&
                    c.Name.ToLower() == category.Name.ToLower());

            if (existed)
            {
                ModelState.AddModelError("Name", "Tên danh mục đã tồn tại.");
                return View(category);
            }

            currentCategory.Name = category.Name;
            currentCategory.IsActive = category.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật danh mục.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(Index));
            }

            if (category.Posts != null && category.Posts.Any())
            {
                category.IsActive = false;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Danh mục đang có tin đăng nên đã được chuyển sang trạng thái tắt.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa danh mục.";
            return RedirectToAction(nameof(Index));
        }
    }
}