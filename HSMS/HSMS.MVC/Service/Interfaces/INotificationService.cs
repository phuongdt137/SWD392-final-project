using HSMS.MVC.Models;

namespace HSMS.MVC.Service;

/// <summary>
/// Giao diện gửi thông báo liên quan đến điểm số.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Gửi thông báo cho học sinh và phụ huynh khi điểm bị thay đổi.
    /// Lỗi gửi thông báo không làm thất bại thao tác cập nhật điểm.
    /// </summary>
    Task NotifyScoreChangedAsync(StudentScore score, decimal? oldScore, long senderUserId);
}
