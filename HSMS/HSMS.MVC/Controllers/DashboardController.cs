using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using HSMS.MVC.DTOs.Auth;

namespace HSMS.MVC.Controllers;

public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ILogger<DashboardController> logger)
    {
        _logger = logger;
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
    public IActionResult Principal()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Dashboard/Principal" });
        }

        return View(user);
    }

    [HttpGet]
    public IActionResult Staff()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Dashboard/Staff" });
        }

        return View(user);
    }

    [HttpGet]
    public IActionResult Teacher()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Dashboard/Teacher" });
        }

        return View(user);
    }

    [HttpGet]
    public IActionResult Student()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Dashboard/Student" });
        }

        return View(user);
    }

    [HttpGet]
    public IActionResult Parent()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Dashboard/Parent" });
        }

        return View(user);
    }
}
