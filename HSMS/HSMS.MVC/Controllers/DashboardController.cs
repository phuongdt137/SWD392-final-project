using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Service;
using Microsoft.AspNetCore.Authorization;

namespace HSMS.MVC.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;
    private readonly IAttendanceService _attendance;

    public DashboardController(ILogger<DashboardController> logger, IAttendanceService attendance)
    {
        _logger = logger;
        _attendance = attendance;
    }

    private LoginResponseDto? GetCurrentUser()
    {
        var userJson = HttpContext.Session.GetString(AuthController.SessionUserKey);
        if (string.IsNullOrEmpty(userJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<LoginResponseDto>(userJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi giải mã JSON thông tin người dùng từ Session: {UserJson}", userJson);
            return null;
        }
    }

    [HttpGet]
    public IActionResult Index()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Dashboard" });
        }

        var primaryRole = user.Roles.FirstOrDefault()?.ToUpper();

        return primaryRole switch
        {
            "PRINCIPAL" => RedirectToAction(nameof(Principal)),
            "STAFF" => RedirectToAction(nameof(Staff)),
            "TEACHER" => RedirectToAction(nameof(Teacher)),
            "STUDENT" => RedirectToAction(nameof(Student)),
            "PARENT" => RedirectToAction(nameof(Parent)),
            _ => RedirectToAction(nameof(Staff))
        };
    }

    [HttpGet]
    public async Task<IActionResult> Principal()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Dashboard/Principal" });
        }

        ViewData["AttendanceSummary"] = await _attendance.DashboardAsync(HttpContext.RequestAborted);
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Staff()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Dashboard/Staff" });
        }

        ViewData["AttendanceSummary"] = await _attendance.DashboardAsync(HttpContext.RequestAborted);
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Teacher()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Dashboard/Teacher" });
        }

        ViewData["AttendanceSummary"] = await _attendance.DashboardAsync(HttpContext.RequestAborted);
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Student()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Dashboard/Student" });
        }

        ViewData["AttendanceSummary"] = await _attendance.DashboardAsync(HttpContext.RequestAborted);
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Parent()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Dashboard/Parent" });
        }

        ViewData["AttendanceSummary"] = await _attendance.DashboardAsync(HttpContext.RequestAborted);
        return View(user);
    }
}
