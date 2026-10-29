using HSMS.MVC.DTOs.Academic;
using HSMS.MVC.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.MVC.Controllers
{
    // Chỉ STAFF hoặc ADMIN được vào
    [Authorize(Roles = "STAFF,ADMIN")]
    public class AcademicYearController : Controller
    {
        private readonly IAcademicYearService _service;

        public AcademicYearController(IAcademicYearService service)
        {
            _service = service;
        }

        // GET /AcademicYear
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _service.GetAllAsync();
            return View(result.Data);
        }

        // GET /AcademicYear/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new SaveAcademicYearDto());
        }

        // POST /AcademicYear/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveAcademicYearDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _service.CreateAsync(dto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message!);
                return View(dto);
            }

            TempData["SuccessMessage"] = "Tạo năm học thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET /AcademicYear/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound();

            var year = result.Data!;
            ViewBag.Semesters = year.Semesters;
            return View(new SaveAcademicYearDto
            {
                Id = year.Id,
                YearCode = year.YearCode,
                StartDate = year.StartDate,
                EndDate = year.EndDate
            });
        }

        // POST /AcademicYear/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SaveAcademicYearDto dto)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.UpdateAsync(dto);
                if (result.StatusCode == 404)
                    return NotFound();

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Cập nhật năm học thành công.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, result.Message!);
            }

            // Form bị lỗi: nạp lại danh sách học kỳ để trang vẫn hiển thị đủ
            var detail = await _service.GetByIdAsync(dto.Id);
            if (!detail.IsSuccess)
                return NotFound();

            ViewBag.Semesters = detail.Data!.Semesters;
            return View(dto);
        }

        // GET /AcademicYear/CreateSemester?academicYearId=5
        [HttpGet]
        public async Task<IActionResult> CreateSemester(long academicYearId)
        {
            var result = await _service.GetByIdAsync(academicYearId);
            if (!result.IsSuccess)
                return NotFound();

            ViewBag.Year = result.Data;
            return View(new SaveSemesterDto { AcademicYearId = academicYearId });
        }

        // POST /AcademicYear/CreateSemester
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSemester(SaveSemesterDto dto)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.CreateSemesterAsync(dto);
                if (result.StatusCode == 404)
                    return NotFound();

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Thêm học kỳ thành công.";
                    return RedirectToAction(nameof(Edit), new { id = dto.AcademicYearId });
                }

                ModelState.AddModelError(string.Empty, result.Message!);
            }

            var year = await _service.GetByIdAsync(dto.AcademicYearId);
            if (!year.IsSuccess)
                return NotFound();

            ViewBag.Year = year.Data;
            return View(dto);
        }
    }
}
