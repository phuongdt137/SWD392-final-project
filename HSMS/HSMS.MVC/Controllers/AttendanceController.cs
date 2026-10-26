using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.MVC.Controllers;

[Authorize]
public sealed class AttendanceController(IAttendanceService attendance) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(AttendanceScheduleFilter filter, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Bộ lọc điểm danh không hợp lệ.");
        if (!User.IsInRole("TEACHER") && !User.IsInRole("ADMIN") && !User.IsInRole("STAFF") && !User.IsInRole("PRINCIPAL"))
            return RedirectToAction(nameof(History));
        var result = await attendance.IndexAsync(filter, ct);
        return result.IsSuccess ? View(result.Data) : Failure(result);
    }
    [HttpGet]
    public async Task<IActionResult> Mark(long timetableEntryId, DateTime date, string? returnUrl, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Ngày học không hợp lệ.");
        var result = await attendance.SheetAsync(timetableEntryId, date, ct);
        ViewData["AttendanceReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action(nameof(Index), new { date });
        return result.IsSuccess ? View(result.Data) : Failure(result);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(SaveAttendanceDto input, string? returnUrl, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            var result = await attendance.SaveAsync(input, ct);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = $"Đã lưu {result.Data!.Changed} kết quả thay đổi.";
                if (result.Data.MissingParents > 0) TempData["WarningMessage"] = $"{result.Data.MissingParents} kết quả chưa có phụ huynh hoạt động được liên kết để nhận thông báo.";
                return RedirectToAction(nameof(Mark), new { input.TimetableEntryId, input.Date, ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null });
            }
            if (result.StatusCode is 401 or 403 or 404) return Failure(result);
            ModelState.AddModelError("", result.Error!.Description);
            Response.StatusCode = result.StatusCode;
        }
        var sheet = await attendance.SheetAsync(input.TimetableEntryId, input.Date, ct);
        if (!sheet.IsSuccess) return Failure(sheet);
        sheet.Data!.Attempt = input;
        ViewData["AttendanceReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action(nameof(Index), new { date = input.Date });
        return View("Mark", sheet.Data);
    }
    [HttpGet]
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
    public async Task<IActionResult> Audit(long id, CancellationToken ct)
    {
        var result = await attendance.AuditAsync(id, ct);
        return result.IsSuccess ? View(result.Data) : Failure(result);
    }
    private IActionResult Failure(ServiceResult result) => StatusCode(result.StatusCode, result.Error?.Description);
}
