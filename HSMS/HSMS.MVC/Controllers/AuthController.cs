using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Service;
using HSMS.MVC.ViewModels.Auth;

namespace HSMS.MVC.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var user = GetCurrentUserFromSession();
        if (user != null)
        {
            return RedirectToDefaultDashboard();
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var loginDto = new LoginDto(model.UsernameOrEmail, model.Password, model.RememberMe);
        var result = await _authService.LoginAsync(loginDto);

        if (!result.IsSuccess || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Description ?? "Đăng nhập thất bại.");
            return View(model);
        }

        var userJson = JsonSerializer.Serialize(result.Data);
        HttpContext.Session.SetString(SessionKeys.CurrentUser, userJson);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) 
            && Url.IsLocalUrl(model.ReturnUrl)
            && !model.ReturnUrl.StartsWith("/Auth/Login", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToRoleDashboard(result.Data.Roles);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult LogoutRedirect()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        var user = GetCurrentUserFromSession();
        if (user == null)
        {
            return RedirectToAction("Login", new { returnUrl = "/Auth/ChangePassword" });
        }

        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var user = GetCurrentUserFromSession();
        if (user == null)
        {
            return RedirectToAction("Login", new { returnUrl = "/Auth/ChangePassword" });
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var dto = new ChangePasswordDto(user.Id, model.CurrentPassword, model.NewPassword);
        var result = await _authService.ChangePasswordAsync(dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Description ?? "Đổi mật khẩu thất bại.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
        return RedirectToAction("ChangePassword");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var baseUrl = Url.Action("ResetPassword", "Auth", null, Request.Scheme) 
                      ?? $"{Request.Scheme}://{Request.Host}/Auth/ResetPassword";

        var dto = new ForgotPasswordDto(model.Email);
        var result = await _authService.ForgotPasswordAsync(dto, baseUrl);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Description ?? "Yêu cầu thất bại.");
            return View(model);
        }

        ViewBag.SuccessMessage = result.Errors.FirstOrDefault() ?? "Liên kết đặt lại mật khẩu đã được gửi đến email của bạn.";
        return View(new ForgotPasswordViewModel());
    }

    [HttpGet]
    public IActionResult ResetPassword(string? token, string? email)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
        {
            TempData["ErrorMessage"] = "Liên kết đặt lại mật khẩu không hợp lệ.";
            return RedirectToAction("Login");
        }

        return View(new ResetPasswordViewModel
        {
            Token = token,
            Email = email
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var dto = new ResetPasswordDto(model.Token, model.Email, model.NewPassword);
        var result = await _authService.ResetPasswordAsync(dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Description ?? "Đặt lại mật khẩu thất bại.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private LoginResponseDto? GetCurrentUserFromSession()
    {
        var userJson = HttpContext.Session.GetString(SessionKeys.CurrentUser);
        if (string.IsNullOrEmpty(userJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<LoginResponseDto>(userJson);
        }
        catch
        {
            return null;
        }
    }

    private IActionResult RedirectToDefaultDashboard()
    {
        var user = GetCurrentUserFromSession();
        if (user?.Roles != null && user.Roles.Count > 0)
        {
            return RedirectToRoleDashboard(user.Roles);
        }

        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToRoleDashboard(IReadOnlyList<string> roles)
    {
        if (roles == null || roles.Count == 0)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        if (roles.Contains("PRINCIPAL"))
        {
            return RedirectToAction("Principal", "Dashboard");
        }

        if (roles.Contains("STAFF"))
        {
            return RedirectToAction("Staff", "Dashboard");
        }

        if (roles.Contains("TEACHER"))
        {
            return RedirectToAction("Teacher", "Dashboard");
        }

        if (roles.Contains("STUDENT"))
        {
            return RedirectToAction("Student", "Dashboard");
        }

        if (roles.Contains("PARENT"))
        {
            return RedirectToAction("Parent", "Dashboard");
        }

        return RedirectToAction("Index", "Dashboard");
    }
}

