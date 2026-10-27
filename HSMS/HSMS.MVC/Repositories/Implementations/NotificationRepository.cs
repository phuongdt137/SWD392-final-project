using HSMS.MVC.Data;
using HSMS.MVC.DTOs.Notifications;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations;

public sealed class NotificationRepository(AppDbContext db) : INotificationRepository
{
    // Chỉ người nhận được xem; thông báo Attendance còn cần liên kết phụ huynh và role hợp lệ hiện tại.
    private IQueryable<Notification> Visible(long userId) => db.Notifications.Where(n => n.RecipientId == userId &&
        (n.AttendanceAuditLogId == null || (n.AttendanceAuditLog!.AttendanceRecord.Customer.Parent != null &&
        n.AttendanceAuditLog.AttendanceRecord.Customer.Parent.UserId == userId &&
        n.Recipient.IsActive && n.Recipient.UserRoles.Any(r => r.IsActive && r.Role.IsActive && r.Role.RoleCode == "PARENT"))));
    // Đếm toàn bộ số chưa đọc trong phạm vi, nhưng chỉ trả tối đa 100 thông báo mới nhất.
    public async Task<NotificationFeed> FeedAsync(long userId, CancellationToken ct)
    {
        var q = Visible(userId).AsNoTracking();
        var unread = await q.CountAsync(x => !x.IsRead, ct);
        var items = await q.OrderByDescending(x => x.SentAt).ThenByDescending(x => x.Id).Take(100)
            .Select(x => new NotificationItem(x.Id, x.Title, x.Content, x.IsRead, x.SentAt, x.Link,
                x.MessageLogs.Where(m => m.Channel == "EMAIL" && m.TemplateCode == "ATTENDANCE")
                    .OrderByDescending(m => m.Id).Select(m => m.Status).FirstOrDefault())).ToListAsync(ct);
        return new(unread, items);
    }
    // Đếm trực tiếp tại database để layout không cần tải nội dung thông báo.
    public Task<int> UnreadCountAsync(long userId, CancellationToken ct) =>
        Visible(userId).CountAsync(x => !x.IsRead, ct);
    // Tìm bằng Visible để không đánh dấu thông báo của người khác; gọi lặp không đổi thời điểm đọc cũ.
    public async Task<bool> ReadAsync(long userId, long id, CancellationToken ct)
    {
        var item = await Visible(userId).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item == null) return false;
        if (!item.IsRead) { item.MarkAsRead(); await db.SaveChangesAsync(ct); }
        return true;
    }
    // Chỉ xử lý thông báo có email được xếp hàng; không gửi bù email cho các thông báo real-time cũ.
    // Nạp người nhận hiện tại và MessageLog để kiểm tra địa chỉ, ghi trạng thái gửi/thử lại.
    public Task<List<Notification>> DueAsync(DateTime utcNow, CancellationToken ct) => db.Notifications
        .Include(x => x.Recipient).Include(x => x.MessageLogs)
        .Where(x => x.AttendanceAuditLogId != null && x.DispatchedAt == null &&
            x.MessageLogs.Any(m => m.Channel == "EMAIL" && m.TemplateCode == "ATTENDANCE") &&
            (x.NextDeliveryAttemptAt == null || x.NextDeliveryAttemptAt <= utcNow))
        .OrderBy(x => x.Id).Take(50).ToListAsync(ct);
    // Tái kiểm tra Visible trước khi gửi, tránh gửi thông báo cũ khi con đã đổi liên kết phụ huynh.
    public Task<bool> CanDeliverAsync(Notification notification, CancellationToken ct) => Visible(notification.RecipientId).AnyAsync(x => x.Id == notification.Id, ct);
    // Lưu kết quả xử lý/retry của tác vụ nền, tách biệt transaction đã lưu điểm danh.
    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
