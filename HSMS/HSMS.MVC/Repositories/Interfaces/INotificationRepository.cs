using HSMS.MVC.DTOs.Notifications;
using HSMS.MVC.Models;

namespace HSMS.MVC.Repositories.Interfaces;

// Truy vấn hộp thông báo và hàng chờ gửi Attendance bằng dữ liệu đã lưu trong database.
public interface INotificationRepository
{
    // Feed và Read đều giới hạn theo người nhận hiện tại.
    Task<NotificationFeed> FeedAsync(long userId, CancellationToken ct);
    Task<int> UnreadCountAsync(long userId, CancellationToken ct);
    Task<bool> ReadAsync(long userId, long id, CancellationToken ct);
    // Tác vụ nền đọc lô đến hạn, kiểm tra người nhận rồi lưu kết quả gửi bằng SaveAsync.
    Task<List<Notification>> DueAsync(DateTime utcNow, CancellationToken ct);
    Task<bool> CanDeliverAsync(Notification notification, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}
