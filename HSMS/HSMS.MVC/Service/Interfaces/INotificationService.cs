using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Notifications;
using HSMS.MVC.Models;

namespace HSMS.MVC.Service;

// Hợp đồng đọc hộp thông báo/đánh dấu đã đọc, luôn gắn với tài khoản hiện tại.
public interface INotificationService
{
    Task<ServiceResult<NotificationFeed>> FeedAsync(bool unreadOnly = false, CancellationToken ct = default);
    // Chỉ đếm để render chuông ở layout, không tải cả danh sách hay mở kết nối nền.
    Task<int> UnreadCountAsync(CancellationToken ct = default);
    Task<ServiceResult> ReadAsync(long id, CancellationToken ct = default);
    Task<ServiceResult<int>> ReadAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<NotificationClassOption>> ClassesAsync(CancellationToken ct = default);
    Task<ServiceResult<int>> SendAnnouncementAsync(SendNotificationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gửi thông báo cho học sinh và phụ huynh khi điểm bị thay đổi.
    /// Lỗi gửi thông báo không làm thất bại thao tác cập nhật điểm.
    /// </summary>
    Task NotifyScoreChangedAsync(StudentScore score, decimal? oldScore, long senderUserId);
}
