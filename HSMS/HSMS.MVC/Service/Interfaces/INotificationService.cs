using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Notifications;

namespace HSMS.MVC.Service;

// Hợp đồng đọc hộp thông báo/đánh dấu đã đọc, luôn gắn với tài khoản hiện tại.
public interface INotificationService
{
    Task<ServiceResult<NotificationFeed>> FeedAsync(CancellationToken ct = default);
    // Chỉ đếm để render chuông ở layout, không tải cả danh sách hay mở kết nối nền.
    Task<int> UnreadCountAsync(CancellationToken ct = default);
    Task<ServiceResult> ReadAsync(long id, CancellationToken ct = default);
}
