namespace HSMS.MVC.DTOs.Notifications;

// Dữ liệu render hộp thông báo; trạng thái email lấy từ MessageLog, độc lập với IsRead.
public sealed record NotificationItem(long Id, string Title, string Content, bool IsRead, DateTime SentAt,
    string? Link, string? EmailStatus);
// Số chưa đọc toàn bộ và tập thông báo mới nhất để hiển thị.
public sealed record NotificationFeed(int Unread, IReadOnlyList<NotificationItem> Items);
