using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Data;
using RaoVatWeb.Models;
using RaoVatWeb.ViewModels;

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
                .Include(a => a.ParentArea)
                .OrderBy(a => a.ParentAreaId == null ? 0 : 1)
                .ThenBy(a => a.ParentArea != null ? a.ParentArea.Name : a.Name)
                .ThenBy(a => a.Name)
                .ToListAsync();

            return View(areas);
        }

        public async Task<IActionResult> Create()
        {
            await LoadParentAreasAsync();

            var model = new AdminAreaFormViewModel
            {
                IsActive = true
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminAreaFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadParentAreasAsync(model.ParentAreaId);
                return View(model);
            }

            var area = new Area
            {
                Name = model.Name.Trim(),
                ParentAreaId = model.ParentAreaId,
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now
            };

            _context.Areas.Add(area);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm khu vực thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var area = await _context.Areas.FindAsync(id);

            if (area == null)
            {
                return NotFound();
            }

            var model = new AdminAreaFormViewModel
            {
                AreaId = area.AreaId,
                Name = area.Name,
                ParentAreaId = area.ParentAreaId,
                IsActive = area.IsActive
            };

            await LoadParentAreasAsync(area.ParentAreaId, area.AreaId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminAreaFormViewModel model)
        {
            if (id != model.AreaId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadParentAreasAsync(model.ParentAreaId, model.AreaId);
                return View(model);
            }

            var area = await _context.Areas.FindAsync(id);

            if (area == null)
            {
                return NotFound();
            }

            area.Name = model.Name.Trim();
            area.ParentAreaId = model.ParentAreaId;
            area.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật khu vực thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var area = await _context.Areas.FindAsync(id);

            if (area == null)
            {
                return NotFound();
            }

            area.IsActive = !area.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = area.IsActive
                ? "Đã khôi phục khu vực."
                : "Đã ẩn khu vực.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadParentAreasAsync(int? selectedParentId = null, int? currentAreaId = null)
        {
            var parentNames = new[] { "Miền Nam", "Miền Bắc" };

            var query = _context.Areas
                .Where(a => a.ParentAreaId == null)
                .Where(a => a.IsActive)
                .Where(a => parentNames.Contains(a.Name));

            if (currentAreaId.HasValue)
            {
                query = query.Where(a => a.AreaId != currentAreaId.Value);
            }

            var parents = await query
                .OrderBy(a => a.Name)
                .ToListAsync();

            ViewBag.ParentAreaId = new SelectList(parents, "AreaId", "Name", selectedParentId);
        }
    }
}