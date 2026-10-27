using HSMS.MVC.DTOs.Notifications;
using HSMS.MVC.Models;

namespace HSMS.MVC.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<NotificationFeed> FeedAsync(long accountId, string accountType, CancellationToken ct);
    Task<bool> ReadAsync(long accountId, string accountType, long id, CancellationToken ct);
    Task<List<Notification>> DueAsync(DateTime utcNow, CancellationToken ct);
    Task<bool> CanDeliverAsync(Notification notification, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}
