using HSMS.MVC.Common;
using HSMS.MVC.DTOs;
using HSMS.MVC.Service;
using HSMS.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace HSMS.MVC.Controllers;

[Authorize]
public class TimetableController(ITimetableService service, ICurrentUserService currentUser,
    SchoolTime clock, ILogger<TimetableController> logger) : Controller
{
    [HttpGet, Authorize(Roles = "STAFF")]
    public IActionResult Create() => View(service.GetFormOptions());

    // Sequence 2 -> 3 -> 19: nhận JSON, gọi service, trả mã HTTP.
    [HttpPost("/api/teaching-assignment"), Authorize(Roles = "STAFF"), ValidateAntiForgeryToken]
    public IActionResult CreateAssignment([FromBody] CreateAssignmentRequest? request)
    {
        if (request == null || !ModelState.IsValid)
            return BadRequest(new AssignmentResponseDto { Message = "Dữ liệu không hợp lệ. Kiểm tra các trường và ngày tháng." });
        try
        {
            var response = service.CreateAssignmentAndTimetable(request);
            if (response.Success) return StatusCode(201, response);
            return response.IsConflict ? Conflict(response) : BadRequest(response);
        }
        catch (Exception ex) when (ex is DbUpdateException or SqlException)
        {
            logger.LogError(ex, "Không thể lưu thời khóa biểu");
            return StatusCode(503, new AssignmentResponseDto { Message = "Chưa lưu được lịch. Hãy tải lại trang và thử lại." });
        }
    }

    [HttpGet, Authorize(Roles = "STAFF")]
    public IActionResult Index(long? classId, long? employeeId, DateTime? week)
    {
        if (!ModelState.IsValid) return BadRequest("Bộ lọc không hợp lệ.");
        return View(new StaffTimetableViewModel {
            Options = service.GetFormOptions(), ClassId = classId, EmployeeId = employeeId,
            Timetable = service.GetStaffTimetable(classId, employeeId, SafeDate(week))
        });
    }

    [HttpGet, Authorize(Roles = "TEACHER")]
    public async Task<IActionResult> Teacher(DateTime? week)
    {
        if (!ModelState.IsValid) return BadRequest("Ngày không hợp lệ.");
        var user = await currentUser.GetAsync();
        if (user?.EmployeeId == null) return Forbid();
        return View(service.GetTeacherTimetable(user.EmployeeId.Value, SafeDate(week)));
    }

    [HttpGet, Authorize(Roles = "STUDENT")]
    public async Task<IActionResult> Student(DateTime? week)
    {
        if (!ModelState.IsValid) return BadRequest("Ngày không hợp lệ.");
        var user = await currentUser.GetAsync();
        if (user?.CustomerId == null) return Forbid();
        return View(service.GetStudentTimetable(user.CustomerId.Value, SafeDate(week)));
    }

    private DateTime SafeDate(DateTime? date) => date is { Year: >= 1900 and <= 9998 } ? date.Value.Date : clock.Today;
}
