using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Notifications;
using HSMS.MVC.Repositories.Interfaces;

namespace HSMS.MVC.Service;

public sealed class NotificationService(INotificationRepository repo, ICurrentUserService current) : INotificationService
{
    public async Task<ServiceResult<NotificationFeed>> FeedAsync(CancellationToken ct = default)
    {
        var user = await current.GetAsync(ct);
        return user == null ? ServiceResult<NotificationFeed>.Failed(AuthErrors.MissingCredentials) :
            ServiceResult<NotificationFeed>.Success(await repo.FeedAsync(user.UserId, ct));
    }
    public async Task<ServiceResult> ReadAsync(long id, CancellationToken ct = default)
    {
        var user = await current.GetAsync(ct);
        if (user == null) return ServiceResult.Unauthorized();
        return await repo.ReadAsync(user.UserId, id, ct) ? ServiceResult.Success() : ServiceResult.NotFound();
    }
}
