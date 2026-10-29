using HSMS.MVC.Repositories.Interfaces;
using HSMS.MVC.DTOs.Academic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.MVC.Controllers
{
    // Chỉ STAFF hoặc ADMIN được vào
    [Authorize(Roles = "STAFF,ADMIN")]
    public class SubjectController : Controller
    {
        private readonly ISubjectService _service;

        public SubjectController(ISubjectService service)
        {
            _service = service;
        }

        // GET /Subject
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _service.GetAllAsync();
            return View(result.Data);
        }

        // GET /Subject/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new SaveSubjectDto());
        }

        // POST /Subject/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveSubjectDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _service.CreateAsync(dto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message!);
                return View(dto);
            }

            TempData["SuccessMessage"] = "Tạo môn học thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Subject/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound();

            return View(result.Data);
        }

        // POST /Subject/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SaveSubjectDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _service.UpdateAsync(dto);
            if (result.StatusCode == 404)
                return NotFound();

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message!);
                return View(dto);
            }

            TempData["SuccessMessage"] = "Cập nhật môn học thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Subject/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            if (result.StatusCode == 404)
                return NotFound();

            if (result.IsSuccess)
                TempData["SuccessMessage"] = "Đã xóa môn học.";
            else
                TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
