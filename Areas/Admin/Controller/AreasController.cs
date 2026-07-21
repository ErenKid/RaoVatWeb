using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models;

namespace RaoVatWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AreasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AreasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var areas = await _context.Areas
                .OrderBy(a => a.ParentAreaId != null)
                .ThenBy(a => a.Name)
                .ToListAsync();

            var postCounts = await _context.Posts
                .GroupBy(p => p.AreaId)
                .Select(g => new
                {
                    AreaId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.AreaId, x => x.Count);

            var parentNames = areas
                .Where(a => a.ParentAreaId.HasValue)
                .ToDictionary(
                    a => a.AreaId,
                    a => areas.FirstOrDefault(p => p.AreaId == a.ParentAreaId)?.Name ?? "Không có"
                );

            ViewBag.PostCounts = postCounts;
            ViewBag.ParentNames = parentNames;

            return View(areas);
        }

        public async Task<IActionResult> Details(int id)
        {
            var area = await _context.Areas
                .FirstOrDefaultAsync(a => a.AreaId == id);

            if (area == null)
            {
                TempData["Error"] = "Không tìm thấy khu vực.";
                return RedirectToAction(nameof(Index));
            }

            var posts = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.AreaId == id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var childAreas = await _context.Areas
                .Where(a => a.ParentAreaId == id)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var parentName = "Khu vực cấp cha";

            if (area.ParentAreaId.HasValue)
            {
                parentName = await _context.Areas
                    .Where(a => a.AreaId == area.ParentAreaId.Value)
                    .Select(a => a.Name)
                    .FirstOrDefaultAsync() ?? "Không có";
            }

            ViewBag.Posts = posts;
            ViewBag.ChildAreas = childAreas;
            ViewBag.ParentName = parentName;

            return View(area);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.ParentAreas = await _context.Areas
                .Where(a => a.ParentAreaId == null && a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var area = new Area
            {
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            return View(area);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,ParentAreaId,IsActive")] Area area)
        {
            if (area.ParentAreaId == 0)
            {
                area.ParentAreaId = null;
            }

            area.Name = area.Name?.Trim() ?? string.Empty;
            area.CreatedAt = DateTime.Now;

            if (!ModelState.IsValid)
            {
                ViewBag.ParentAreas = await _context.Areas
                    .Where(a => a.ParentAreaId == null && a.IsActive)
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return View(area);
            }

            var existed = await _context.Areas
                .AnyAsync(a =>
                    a.Name.ToLower() == area.Name.ToLower() &&
                    a.ParentAreaId == area.ParentAreaId);

            if (existed)
            {
                ModelState.AddModelError("Name", "Tên khu vực đã tồn tại trong cùng cấp.");

                ViewBag.ParentAreas = await _context.Areas
                    .Where(a => a.ParentAreaId == null && a.IsActive)
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return View(area);
            }

            _context.Areas.Add(area);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã thêm khu vực mới.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var area = await _context.Areas
                .FirstOrDefaultAsync(a => a.AreaId == id);

            if (area == null)
            {
                TempData["Error"] = "Không tìm thấy khu vực.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ParentAreas = await _context.Areas
                .Where(a =>
                    a.ParentAreaId == null &&
                    a.AreaId != id &&
                    a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return View(area);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AreaId,Name,ParentAreaId,IsActive,CreatedAt")] Area area)
        {
            if (id != area.AreaId)
            {
                TempData["Error"] = "Dữ liệu khu vực không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (area.ParentAreaId == 0)
            {
                area.ParentAreaId = null;
            }

            area.Name = area.Name?.Trim() ?? string.Empty;

            if (!ModelState.IsValid)
            {
                ViewBag.ParentAreas = await _context.Areas
                    .Where(a =>
                        a.ParentAreaId == null &&
                        a.AreaId != id &&
                        a.IsActive)
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return View(area);
            }

            var currentArea = await _context.Areas
                .FirstOrDefaultAsync(a => a.AreaId == id);

            if (currentArea == null)
            {
                TempData["Error"] = "Không tìm thấy khu vực.";
                return RedirectToAction(nameof(Index));
            }

            var existed = await _context.Areas
                .AnyAsync(a =>
                    a.AreaId != id &&
                    a.Name.ToLower() == area.Name.ToLower() &&
                    a.ParentAreaId == area.ParentAreaId);

            if (existed)
            {
                ModelState.AddModelError("Name", "Tên khu vực đã tồn tại trong cùng cấp.");

                ViewBag.ParentAreas = await _context.Areas
                    .Where(a =>
                        a.ParentAreaId == null &&
                        a.AreaId != id &&
                        a.IsActive)
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return View(area);
            }

            currentArea.Name = area.Name;
            currentArea.ParentAreaId = area.ParentAreaId;
            currentArea.IsActive = area.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật khu vực.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var area = await _context.Areas
                .FirstOrDefaultAsync(a => a.AreaId == id);

            if (area == null)
            {
                TempData["Error"] = "Không tìm thấy khu vực.";
                return RedirectToAction(nameof(Index));
            }

            var hasPosts = await _context.Posts
                .AnyAsync(p => p.AreaId == id);

            var hasChildren = await _context.Areas
                .AnyAsync(a => a.ParentAreaId == id);

            if (hasPosts || hasChildren)
            {
                area.IsActive = false;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Khu vực đang có dữ liệu liên quan nên đã được chuyển sang trạng thái tắt.";
                return RedirectToAction(nameof(Index));
            }

            _context.Areas.Remove(area);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa khu vực.";
            return RedirectToAction(nameof(Index));
        }
    }
}