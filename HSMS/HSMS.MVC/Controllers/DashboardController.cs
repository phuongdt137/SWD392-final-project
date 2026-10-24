using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using HSMS.MVC.DTOs.Auth;

namespace HSMS.MVC.Controllers;

public class DashboardController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var userJson = HttpContext.Session.GetString(AuthController.SessionUserKey);
        if (string.IsNullOrEmpty(userJson))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Dashboard" });
        }

        try
        {
            var user = JsonSerializer.Deserialize<LoginResponseDto>(userJson);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(user);
        }
        catch
        {
            return RedirectToAction("Login", "Auth");
        }
    }
}
