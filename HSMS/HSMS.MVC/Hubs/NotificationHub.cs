using HSMS.MVC.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HSMS.MVC.Hubs;

[Authorize(Roles = "PARENT")]
public sealed class NotificationHub(ICurrentUserService current) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var user = await current.GetAsync(Context.ConnectionAborted);
        if (user is not { CustomerId: not null } || !user.Has("PARENT")) { Context.Abort(); return; }
        await base.OnConnectedAsync();
    }
    // Recipients come exclusively from the authenticated NameIdentifier claim.
}
