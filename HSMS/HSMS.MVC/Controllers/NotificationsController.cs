using HSMS.MVC.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.MVC.Controllers;

[Authorize]
public sealed class NotificationsController(INotificationService notifications) : Controller
{
    [HttpGet]
    // Hộp thông báo HTML thông thường, dữ liệu được cập nhật khi mở/tải lại trang.
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var result = await notifications.FeedAsync(ct);
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
}
