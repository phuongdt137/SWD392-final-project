using System.Security.Claims;
using System.Text.Json;
using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HSMS.MVC.Service;

/// <summary>Refresh active roles and profiles instead of trusting an old login snapshot.</summary>
public sealed class SchoolCookieEvents(IUserRepository users) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (!long.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        { context.RejectPrincipal(); return; }
        var user = await users.GetByIdWithDetailsAsync(id);
        if (user is not { IsActive: true } || user.GetActiveRoleCodes().Count == 0)
        {
            context.RejectPrincipal(); context.HttpContext.Session.Clear();
            await context.HttpContext.SignOutAsync(); return;
        }
        context.ReplacePrincipal(UserPrincipal.Create(user.Id, user.FullName, user.GetActiveRoleCodes()));
        var dto = new LoginResponseDto(user.Id, user.Username, user.FullName, user.Email,
            user.EmployeeProfile == null ? "CUSTOMER" : "EMPLOYEE", user.GetActiveRoleCodes(), user.AvatarUrl);
        context.HttpContext.Session.SetString("CurrentUser", JsonSerializer.Serialize(dto));
    }
    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        if (IsEndpoint(context.Request.Path)) context.Response.StatusCode = 401;
        else context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    }
    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        if (IsEndpoint(context.Request.Path)) context.Response.StatusCode = 403;
        else context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    }
    private static bool IsEndpoint(PathString path) => path.StartsWithSegments("/hubs") || path.StartsWithSegments("/Notifications/Feed");
}
