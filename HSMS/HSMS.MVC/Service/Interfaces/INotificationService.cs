using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Notifications;
using HSMS.MVC.Models;

namespace HSMS.MVC.Service;

public interface INotificationService
{
    Task<ServiceResult<NotificationFeed>> FeedAsync(CancellationToken ct = default);
    Task<ServiceResult> ReadAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Gửi thông báo cho học sinh và phụ huynh khi điểm bị thay đổi.
    /// Lỗi gửi thông báo không làm thất bại thao tác cập nhật điểm.
    /// </summary>
    Task NotifyScoreChangedAsync(StudentScore score, decimal? oldScore, long senderEmployeeId);
}
