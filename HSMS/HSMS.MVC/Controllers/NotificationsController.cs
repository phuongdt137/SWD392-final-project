using HSMS.MVC.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.MVC.Controllers;

[Authorize]
public sealed class NotificationsController(INotificationService notifications) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var result = await notifications.FeedAsync(ct);
        return result.IsSuccess ? View(result.Data) : StatusCode(result.StatusCode);
    }
    [HttpGet]
    public async Task<IActionResult> Feed(CancellationToken ct)
    {
        var result = await notifications.FeedAsync(ct);
        return result.IsSuccess ? Json(result.Data) : StatusCode(result.StatusCode);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Read(long id, CancellationToken ct)
    {
        var result = await notifications.ReadAsync(id, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode);
        return RedirectToAction(nameof(Index));
    }
}
