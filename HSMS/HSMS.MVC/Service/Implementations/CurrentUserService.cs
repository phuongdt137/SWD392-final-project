using System.Security.Claims;
using HSMS.MVC.Common;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
namespace HSMS.MVC.Service;

public sealed class CurrentUserService(IHttpContextAccessor http, IAccountRepository accounts) : ICurrentUserService
{
    public async Task<CurrentUser?> GetAsync(CancellationToken cancellationToken = default)
    {
        var principal = http.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true || !long.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return null;
        var account = await accounts.GetAsync(id, principal.FindFirstValue(UserPrincipal.AccountTypeClaim) ?? "");
        if (account is not { IsActive: true }) return null;
        return new(account.Id, account.FullName, account is Employee ? account.Id : null,
            account is Customer ? account.Id : null, account.GetActiveRoleCodes());
    }
}
