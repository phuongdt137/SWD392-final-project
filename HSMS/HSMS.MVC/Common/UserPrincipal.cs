using System.Security.Claims;
using HSMS.MVC.DTOs.Auth;

namespace HSMS.MVC.Common;

public static class UserPrincipal
{
    // Principal chỉ tồn tại trong request, được dựng từ Session đã kiểm tra với database.
    // Không phát hành vé/cookie xác thực riêng; [Authorize] và IsInRole vẫn dùng được.
    public const string SessionScheme = "Session";

    public static ClaimsPrincipal Create(long id, string name, IEnumerable<string> roles) => new(
        new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Name, name) }
            .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))), SessionScheme));
    public static ClaimsPrincipal Create(LoginResponseDto user) => Create(user.Id, user.FullName, user.Roles);
}
