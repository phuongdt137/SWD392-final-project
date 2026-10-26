using System.Security.Claims;
using HSMS.MVC.Repositories.Interfaces;

namespace HSMS.MVC.Service;

public sealed class CurrentUserService(IHttpContextAccessor http, IUserRepository users) : ICurrentUserService
{
    public async Task<CurrentUser?> GetAsync(CancellationToken cancellationToken = default)
    {
        if (!long.TryParse(http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return null;
        var user = await users.GetByIdWithDetailsAsync(id);
        if (user is not { IsActive: true }) return null;
        return new(user.Id, user.FullName, user.EmployeeProfile?.Id, user.CustomerProfile?.Id, user.GetActiveRoleCodes());
    }
}
