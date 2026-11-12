using HSMS.MVC.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HSMS.MVC.DTOs.Notifications;

namespace HSMS.MVC.Controllers;

[Authorize]
public sealed class NotificationsController(INotificationService notifications) : Controller
{
    [HttpGet]
    // Hộp thông báo HTML thông thường, dữ liệu được cập nhật khi mở/tải lại trang.
    public async Task<IActionResult> Index(string filter = "all", CancellationToken ct = default)
    {
        filter = filter.Equals("unread", StringComparison.OrdinalIgnoreCase) ? "unread" : "all";
        ViewBag.Filter = filter;
        var result = await notifications.FeedAsync(filter == "unread", ct);
        return result.IsSuccess ? View(result.Data) : StatusCode(result.StatusCode);
    }
    [HttpPost, ValidateAntiForgeryToken]
    // POST có chống giả mạo biểu mẫu; đánh dấu đã đọc rồi chuyển về hộp thông báo.
    public async Task<IActionResult> Read(long id, CancellationToken ct)
    {
        var result = await notifications.ReadAsync(id, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReadAll(CancellationToken ct)
    {
        var result = await notifications.ReadAllAsync(ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode);
        TempData["SuccessMessage"] = $"Đã đánh dấu {result.Data} thông báo là đã đọc.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "STAFF")]
    [HttpGet]
    public async Task<IActionResult> Send(CancellationToken ct)
    {
        ViewBag.Classes = await notifications.ClassesAsync(ct);
        return View(new SendNotificationRequest { TargetType = "ALL" });
    }

    [Authorize(Roles = "STAFF")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(SendNotificationRequest request, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            var result = await notifications.SendAnnouncementAsync(request, ct);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = $"Đã gửi thông báo đến {result.Data} người nhận.";
                return RedirectToAction(nameof(Send));
            }
            ModelState.AddModelError(string.Empty, result.Message ?? "Không thể gửi thông báo.");
        }
        ViewBag.Classes = await notifications.ClassesAsync(ct);
        return View(request);
    }
}
