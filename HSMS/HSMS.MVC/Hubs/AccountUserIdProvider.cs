using System.Security.Claims;
using HSMS.MVC.Common;
using Microsoft.AspNetCore.SignalR;
namespace HSMS.MVC.Hubs;

public sealed class AccountUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var type = connection.User.FindFirstValue(UserPrincipal.AccountTypeClaim);
        var id = connection.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return type is "EMPLOYEE" or "CUSTOMER" && id != null ? type + ":" + id : null;
    }
}
