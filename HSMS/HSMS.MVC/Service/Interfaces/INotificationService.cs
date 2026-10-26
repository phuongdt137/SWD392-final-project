using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Notifications;

namespace HSMS.MVC.Service;

public interface INotificationService
{
    Task<ServiceResult<NotificationFeed>> FeedAsync(CancellationToken ct = default);
    Task<ServiceResult> ReadAsync(long id, CancellationToken ct = default);
}
