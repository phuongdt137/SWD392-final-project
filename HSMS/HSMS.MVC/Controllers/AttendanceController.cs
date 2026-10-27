using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.MVC.Controllers;

// Mọi action cần đăng nhập; Service tiếp tục kiểm tra quyền trên từng lớp/học sinh.
[Authorize]
public sealed class AttendanceController(IAttendanceService attendance) : Controller
{
    [HttpGet]
    // GET /Attendance: danh sách tiết học và bộ lọc cho giáo viên/nhóm quản lý.
    public async Task<IActionResult> Index(AttendanceScheduleFilter filter, CancellationToken ct)
    {
        // ModelState chứa lỗi chuyển kiểu và validation dữ liệu request; có lỗi thì trả HTTP 400.
        if (!ModelState.IsValid) return BadRequest("Bộ lọc điểm danh không hợp lệ.");
        // Dẫn học sinh/phụ huynh sang lịch sử; chuyển hướng không thay thế kiểm tra quyền.
        if (!User.IsInRole("TEACHER") && !User.IsInRole("ADMIN") && !User.IsInRole("STAFF") && !User.IsInRole("PRINCIPAL"))
            return RedirectToAction(nameof(History));
        var result = await attendance.IndexAsync(filter, ct);
        return result.IsSuccess ? View(result.Data) : Failure(result);
    }
    [HttpGet]
    // GET /Attendance/Mark: Service đọc bảng và xác định có được sửa hay chỉ xem.
    public async Task<IActionResult> Mark(long timetableEntryId, DateTime date, string? returnUrl, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Ngày học không hợp lệ.");
        var result = await attendance.SheetAsync(timetableEntryId, date, ct);
        // Chỉ dùng địa chỉ nội bộ cho nút quay lại để tránh chuyển hướng sang website khác.
        ViewData["AttendanceReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action(nameof(Index), new { date });
        return result.IsSuccess ? View(result.Data) : Failure(result);
    }
    [HttpPost, ValidateAntiForgeryToken]
    // POST /Attendance/Save: kiểm tra biểu mẫu rồi chuyển nghiệp vụ lưu cho Service.
    public async Task<IActionResult> Save(SaveAttendanceDto input, string? returnUrl, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            var result = await attendance.SaveAsync(input, ct);
            if (result.IsSuccess)
            {
                // TempData giữ thông báo qua chuyển hướng: POST -> 302 -> GET Mark -> HTML mới.
                TempData["SuccessMessage"] = $"Đã lưu {result.Data!.Changed} kết quả thay đổi.";
                if (result.Data.MissingParents > 0) TempData["WarningMessage"] = $"{result.Data.MissingParents} kết quả chưa có phụ huynh hoạt động được liên kết để nhận thông báo.";
                return RedirectToAction(nameof(Mark), new { input.TimetableEntryId, input.Date, ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null });
            }
            if (result.StatusCode is 401 or 403 or 404) return Failure(result);
            ModelState.AddModelError("", result.Error!.Description);
            Response.StatusCode = result.StatusCode;
        }
        // Khi lỗi nhập liệu/lưu, tải lại dữ liệu nền và giữ Attempt để hiển thị nội dung đã nhập.
        var sheet = await attendance.SheetAsync(input.TimetableEntryId, input.Date, ct);
        if (!sheet.IsSuccess) return Failure(sheet);
        sheet.Data!.Attempt = input;
        ViewData["AttendanceReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action(nameof(Index), new { date = input.Date });
        return View("Mark", sheet.Data);
    }
    [HttpGet]
    // GET /Attendance/History: học sinh/phụ huynh có giao diện riêng và phạm vi dữ liệu riêng.
    public async Task<IActionResult> History(AttendanceFilter filter, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Bộ lọc không hợp lệ.");
        if ((User.IsInRole("STUDENT") || User.IsInRole("PARENT")) && !User.IsInRole("TEACHER") && !User.IsInRole("ADMIN") && !User.IsInRole("STAFF") && !User.IsInRole("PRINCIPAL"))
        {
            var studentResult = await attendance.StudentHistoryAsync(filter, ct);
            return studentResult.IsSuccess ? View("StudentHistory", studentResult.Data) : Failure(studentResult);
        }
        var result = await attendance.HistoryAsync(filter, ct);
        return result.IsSuccess ? View(result.Data) : Failure(result);
    }
    [HttpGet]
    // Đọc nhật ký một kết quả; Service kiểm tra quyền và phạm vi trước khi trả dữ liệu.
    public async Task<IActionResult> Audit(long id, CancellationToken ct)
    {
        var result = await attendance.AuditAsync(id, ct);
        return result.IsSuccess ? View(result.Data) : Failure(result);
    }
    // Chuyển mã lỗi nghiệp vụ sang HTTP status tương ứng (ví dụ 403, 404, 409).
    private IActionResult Failure(ServiceResult result) => StatusCode(result.StatusCode, result.Error?.Description);
}
