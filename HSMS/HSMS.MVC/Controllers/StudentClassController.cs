using HSMS.MVC.DTOs.Academic;
using HSMS.MVC.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HSMS.MVC.Controllers
{
    // Chỉ STAFF hoặc ADMIN được vào
    [Authorize(Roles = "STAFF,ADMIN")]
    public class StudentClassController : Controller
    {
        private readonly IStudentClassService _service;

        public StudentClassController(IStudentClassService service)
        {
            _service = service;
        }

        // GET /StudentClass?classId=5  -> chọn lớp, xem học sinh trong lớp
        [HttpGet]
        public async Task<IActionResult> Index(long? classId)
        {
            var options = (await _service.GetClassOptionsAsync()).Data!;
            ViewBag.Classes = options
                .Select(o => new SelectListItem(o.Text, o.Id.ToString(), o.Id == classId))
                .ToList();

            if (!classId.HasValue)
                return View((ClassStudentsDto?)null);

            var result = await _service.GetClassStudentsAsync(classId.Value);
            if (!result.IsSuccess)
                return NotFound();

            return View(result.Data);
        }

        // GET /StudentClass/Assign?classId=5
        [HttpGet]
        public async Task<IActionResult> Assign(long classId)
        {
            var form = await LoadAssignFormAsync(classId);
            if (form.StatusCode == 404)
                return NotFound();

            if (!form.IsSuccess)
            {
                TempData["ErrorMessage"] = form.Message;
                return RedirectToAction(nameof(Index), new { classId });
            }

            return View(new AssignStudentDto { ClassId = classId, JoinedAt = DateTime.Today });
        }

        // POST /StudentClass/Assign
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignStudentDto dto)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.AssignAsync(dto);
                if (result.StatusCode == 404)
                    return NotFound();

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Đã xếp học sinh vào lớp.";
                    return RedirectToAction(nameof(Index), new { classId = dto.ClassId });
                }

                ModelState.AddModelError(string.Empty, result.Message!);
            }

            // Form lỗi: nạp lại thông tin lớp và danh sách học sinh
            var form = await LoadAssignFormAsync(dto.ClassId);
            if (!form.IsSuccess)
                return NotFound();

            return View(dto);
        }

        // GET /StudentClass/Transfer/12   (12 = Id bản ghi StudentClass)
        [HttpGet]
        public async Task<IActionResult> Transfer(long id)
        {
            var form = await LoadTransferFormAsync(id);
            if (form.StatusCode == 404)
                return NotFound();

            if (!form.IsSuccess)
            {
                TempData["ErrorMessage"] = form.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(new TransferStudentDto { StudentClassId = id });
        }

        // POST /StudentClass/Transfer
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(TransferStudentDto dto)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.TransferAsync(dto);
                if (result.StatusCode == 404)
                    return NotFound();

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Đã chuyển lớp. Lịch sử chuyển lớp đã được ghi lại.";
                    return RedirectToAction(nameof(Index), new { classId = result.Data });
                }

                ModelState.AddModelError(string.Empty, result.Message!);
            }

            var form = await LoadTransferFormAsync(dto.StudentClassId);
            if (!form.IsSuccess)
                return NotFound();

            return View(dto);
        }

        // Nạp dữ liệu hiển thị cho form xếp lớp vào ViewBag
        private async Task<HSMS.MVC.Common.ServiceResult<AssignFormDto>> LoadAssignFormAsync(long classId)
        {
            var form = await _service.GetAssignFormAsync(classId);
            if (form.IsSuccess)
            {
                ViewBag.ClassName = form.Data!.ClassName;
                ViewBag.YearCode = form.Data.AcademicYearCode;
                ViewBag.Students = form.Data.Students
                    .Select(s => new SelectListItem(s.Text, s.Id.ToString()))
                    .ToList();
            }
            return form;
        }

        // Nạp dữ liệu hiển thị cho form chuyển lớp vào ViewBag
        private async Task<HSMS.MVC.Common.ServiceResult<TransferFormDto>> LoadTransferFormAsync(long studentClassId)
        {
            var form = await _service.GetTransferFormAsync(studentClassId);
            if (form.IsSuccess)
            {
                ViewBag.StudentName = form.Data!.StudentName;
                ViewBag.StudentCode = form.Data.StudentCode;
                ViewBag.FromClassName = form.Data.FromClassName;
                ViewBag.YearCode = form.Data.AcademicYearCode;
                ViewBag.Classes = form.Data.Classes
                    .Select(c => new SelectListItem(c.Text, c.Id.ToString()))
                    .ToList();
            }
            return form;
        }
    }
}
