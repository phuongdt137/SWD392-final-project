using System.Security.Claims;
using System.Text.Json;
using HSMS.MVC.DTOs.Auth;

namespace HSMS.MVC.Common;

/// <summary>
/// Middleware đọc thông tin người dùng từ Session (dựa theo SessionId trong cookie trình duyệt)
/// và nạp vào HttpContext.User trước khi qua bước UseAuthorization.
/// </summary>
public sealed class SessionAuthMiddleware
{
    private readonly RequestDelegate _next;

    public SessionAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userJson = context.Session.GetString(SessionKeys.CurrentUser);
        if (!string.IsNullOrEmpty(userJson))
        {
            try
            {
                var user = JsonSerializer.Deserialize<LoginResponseDto>(userJson);
                if (user != null && user.Id > 0)
                {
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new(ClaimTypes.Name, user.FullName)
                    };
                    claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

                    context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Session"));
                }
            }
            catch (JsonException)
            {
                context.Session.Remove(SessionKeys.CurrentUser);
            }
        }

        await _next(context);
    }
}

