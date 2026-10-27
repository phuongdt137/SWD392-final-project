using System.Security.Claims;
using HSMS.MVC.DTOs.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
namespace HSMS.MVC.Common;

public static class UserPrincipal
{
    public const string AccountTypeClaim = "AccountType";
    public static ClaimsPrincipal Create(long id, string name, IEnumerable<string> roles, string accountType)
    {
        if (accountType != "EMPLOYEE" && accountType != "CUSTOMER") throw new ArgumentException("Loại tài khoản không hợp lệ.");
        return new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Name, name),
            new Claim(AccountTypeClaim, accountType)
        }.Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))), CookieAuthenticationDefaults.AuthenticationScheme));
    }
    public static ClaimsPrincipal Create(LoginResponseDto account) => Create(account.Id, account.FullName, account.Roles, account.UserType);
}
