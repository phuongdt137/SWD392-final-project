using System.Security.Claims;
using HSMS.MVC.DTOs.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HSMS.MVC.Common;

public static class UserPrincipal
{
    public static ClaimsPrincipal Create(long id, string name, IEnumerable<string> roles) => new(
        new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Name, name) }
            .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))), CookieAuthenticationDefaults.AuthenticationScheme));
    public static ClaimsPrincipal Create(LoginResponseDto user) => Create(user.Id, user.FullName, user.Roles);
}
