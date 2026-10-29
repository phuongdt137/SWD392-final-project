using System.Text.Json;
using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.MVC.Controllers;

[Authorize(Roles = "TEACHER,STAFF,PRINCIPAL,STUDENT,PARENT")]
public class AttendanceController : Controller
{
    private readonly IAttendanceService _attendance;

    /// <summary>Nhận service xử lý điểm danh để các action gọi nghiệp vụ.</summary>
    public AttendanceController(IAttendanceService attendance)
    {
        _attendance = attendance;
    }

    // [Authorize] đã kiểm tra đăng nhập; dùng thông tin do Auth lưu ở Session phía server.
    private LoginResponseDto CurrentUser => JsonSerializer.Deserialize<LoginResponseDto>(
        HttpContext.Session.GetString(SessionKeys.CurrentUser)!)!;

    /// <summary>Hiển thị lịch điểm danh; chuyển học sinh và phụ huynh sang màn hình xem kết quả.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(AttendanceScheduleFilter filter, CancellationToken ct)
    {
        if (!User.IsInRole("TEACHER") && !User.IsInRole("STAFF") && !User.IsInRole("PRINCIPAL"))
            return RedirectToAction(nameof(History));
        if (!ModelState.IsValid) return BadRequest("Bộ lọc lịch học không hợp lệ.");
        var result = await _attendance.IndexAsync(CurrentUser, filter, ct);
        return result.IsSuccess ? View(result.Data) : StatusCode(result.StatusCode, result.Message);
    }

    /// <summary>Mở bảng học sinh và kết quả điểm danh của tiết, ngày được chọn.</summary>
    [HttpGet]
    [Authorize(Roles = "TEACHER,STAFF,PRINCIPAL")]
    public async Task<IActionResult> Mark(long timetableEntryId, DateTime date, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Ngày hoặc tiết học không hợp lệ.");
        var result = await _attendance.SheetAsync(CurrentUser, timetableEntryId, date, ct);
        return result.IsSuccess ? View(result.Data) : StatusCode(result.StatusCode, result.Message);
    }

    /// <summary>Nhận biểu mẫu, gọi lưu điểm danh và trả thông báo thành công hoặc hiển thị lỗi trên bảng.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "TEACHER,STAFF")]
    public async Task<IActionResult> Save(SaveAttendanceDto input, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            var result = await _attendance.SaveAsync(CurrentUser, input, ct);
            if (result.IsSuccess)
            {
                TempData["AttendanceSuccess"] = $"Đã lưu {result.Data!.Changed} kết quả thay đổi.";
                if (result.Data.MissingParents > 0 || result.Data.MissingEmails > 0)
                    TempData["AttendanceWarning"] = $"{result.Data.MissingParents} cảnh báo thiếu phụ huynh hợp lệ; " +
                        $"{result.Data.MissingEmails} cảnh báo đã tạo thông báo nhưng chưa gửi được email do địa chỉ không hợp lệ.";
                return RedirectToAction(nameof(Mark), new { input.TimetableEntryId, Date = input.Date.ToString("yyyy-MM-dd") });
            }
            if (result.StatusCode is 401 or 403 or 404) return StatusCode(result.StatusCode, result.Message);
            ModelState.AddModelError(string.Empty, result.Message ?? "Không thể lưu điểm danh.");
            Response.StatusCode = result.StatusCode;
        }
        else Response.StatusCode = 400;

        var sheet = await _attendance.SheetAsync(CurrentUser, input.TimetableEntryId, input.Date, ct);
        if (!sheet.IsSuccess) return StatusCode(sheet.StatusCode, sheet.Message);
        sheet.Data!.Attempt = input;
        return View("Mark", sheet.Data);
    }

    /// <summary>Hiển thị kết quả điểm danh theo các bộ lọc phù hợp với người dùng.</summary>
    [HttpGet]
    [Authorize(Roles = "STAFF,PRINCIPAL,STUDENT,PARENT")]
    public async Task<IActionResult> History(AttendanceHistoryFilter filter, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Bộ lọc lịch sử không hợp lệ.");
        var result = await _attendance.HistoryAsync(CurrentUser, filter, ct);
        return result.IsSuccess ? View(result.Data) : StatusCode(result.StatusCode, result.Message);
    }

    /// <summary>Hiển thị nhật ký thay đổi của một kết quả cho Staff hoặc Principal.</summary>
    [HttpGet]
    [Authorize(Roles = "STAFF,PRINCIPAL")]
    public async Task<IActionResult> Audit(long id, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Mã kết quả không hợp lệ.");
        var result = await _attendance.AuditAsync(id, ct);
        return result.IsSuccess ? View(result.Data) : StatusCode(result.StatusCode, result.Message);
    }
}
