using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Service;

namespace HSMS.MVC.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IAttendanceService _attendance;

    public DashboardController(IAttendanceService attendance)
    {
        _attendance = attendance;
    }

    private LoginResponseDto GetCurrentUser()
    {
        var userJson = HttpContext.Session.GetString(SessionKeys.CurrentUser)!;
        return JsonSerializer.Deserialize<LoginResponseDto>(userJson)!;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var user = GetCurrentUser();
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

    [Authorize(Roles = "PRINCIPAL")]
    [HttpGet]
    public async Task<IActionResult> Principal()
    {
        var user = GetCurrentUser();
        ViewData["AttendanceSummary"] = await _attendance.DashboardAsync(HttpContext.RequestAborted);
        return View(user);
    }

    [Authorize(Roles = "STAFF")]
    [HttpGet]
    public async Task<IActionResult> Staff()
    {
        var user = GetCurrentUser();
        ViewData["AttendanceSummary"] = await _attendance.DashboardAsync(HttpContext.RequestAborted);
        return View(user);
    }

    [Authorize(Roles = "TEACHER")]
    [HttpGet]
    public async Task<IActionResult> Teacher()
    {
        var user = GetCurrentUser();
        ViewData["AttendanceSummary"] = await _attendance.DashboardAsync(HttpContext.RequestAborted);
        return View(user);
    }

    [Authorize(Roles = "STUDENT")]
    [HttpGet]
    public async Task<IActionResult> Student()
    {
        var user = GetCurrentUser();
        ViewData["AttendanceSummary"] = await _attendance.DashboardAsync(HttpContext.RequestAborted);
        return View(user);
    }

    [Authorize(Roles = "PARENT")]
    [HttpGet]
    public async Task<IActionResult> Parent()
    {
        var user = GetCurrentUser();
        ViewData["AttendanceSummary"] = await _attendance.DashboardAsync(HttpContext.RequestAborted);
        return View(user);
    }
}

