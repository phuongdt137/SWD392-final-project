using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Auth;

namespace HSMS.MVC.Controllers;

[Authorize]
public class DashboardController : Controller
{
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
    public IActionResult Principal()
    {
        var user = GetCurrentUser();
        return View(user);
    }

    [Authorize(Roles = "STAFF")]
    [HttpGet]
    public IActionResult Staff()
    {
        var user = GetCurrentUser();
        return View(user);
    }

    [Authorize(Roles = "TEACHER")]
    [HttpGet]
    public IActionResult Teacher()
    {
        var user = GetCurrentUser();
        return View(user);
    }

    [Authorize(Roles = "STUDENT")]
    [HttpGet]
    public IActionResult Student()
    {
        var user = GetCurrentUser();
        return View(user);
    }

    [Authorize(Roles = "PARENT")]
    [HttpGet]
    public IActionResult Parent()
    {
        var user = GetCurrentUser();
        return View(user);
    }
}

