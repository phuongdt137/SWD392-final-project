using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

/// <summary>
/// Thực thể Thông báo trong hệ thống.
/// </summary>
public class Notification
{
    public long Id { get; private set; }
    public long? SenderId { get; private set; }
    // Người nhận là User ID để tra cứu hộp thông báo và email, không phải Customer/Employee ID.
    public long RecipientId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTime SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    // Liên kết lần thay đổi Attendance; thông báo được lưu cùng transaction với kết quả và audit.
    public long? AttendanceAuditLogId { get; private set; }
    public virtual AttendanceAuditLog? AttendanceAuditLog { get; private set; }
    // Trạng thái xử lý hàng chờ email khác IsRead; dùng lại các cột gửi nền đã có.
    public DateTime? DispatchedAt { get; private set; }
    public int DeliveryAttempts { get; private set; }
    public DateTime? NextDeliveryAttemptAt { get; private set; }
    public string? Link { get; private set; }

    public virtual User? Sender { get; private set; }
    public virtual User Recipient { get; private set; } = null!;
    public virtual ICollection<MessageLog> MessageLogs { get; private set; } = new List<MessageLog>();

    protected Notification() { }

    public Notification(string title, string content, long recipientId, long? senderId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new AppError("Tiêu đề thông báo không được để trống.", "TITLE_REQUIRED");

        if (string.IsNullOrWhiteSpace(content))
            throw new AppError("Nội dung thông báo không được để trống.", "CONTENT_REQUIRED");

        if (recipientId <= 0)
            throw new AppError("Id người nhận không hợp lệ.", "INVALID_RECIPIENT");

        if (senderId.HasValue && senderId.Value <= 0)
            throw new AppError("Id người gửi không hợp lệ.", "INVALID_SENDER");

        Title = title.Trim();
        Content = content.Trim();
        RecipientId = recipientId;
        SenderId = senderId;
        IsRead = false;
        SentAt = DateTime.UtcNow;
    }

    // Chỉ cập nhật trạng thái entity; SMTP được xử lý bởi dispatcher sau commit.
    public bool SendNotification()
    {
        SentAt = DateTime.UtcNow;
        IsRead = false;
        return true;
    }

    // Chỉ đánh dấu đã đọc khi thao tác trong ứng dụng, không phụ thuộc gửi/đọc email.
    public bool MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        return true;
    }

    // Gắn nguồn thay đổi và lịch gửi đầu tiên; Navigation cho phép EF nối audit mới chưa có ID.
    public void AttachAttendance(AttendanceAuditLog audit, string link, DateTime utcNow)
    {
        AttendanceAuditLog = audit; AttendanceAuditLogId = audit.Id;
        Link = link; SentAt = utcNow; NextDeliveryAttemptAt = utcNow;
    }
    // Kết thúc xử lý hàng chờ (đã gửi hoặc không còn được nhận); MessageLog lưu kết quả chi tiết.
    public void Delivered(DateTime utcNow)
    {
        DispatchedAt = utcNow; DeliveryAttempts++; NextDeliveryAttemptAt = null;
    }
    // Lỗi gửi được hẹn lại theo thời gian tăng dần, tối đa một giờ giữa các lần thử.
    public void RetryLater(DateTime utcNow)
    {
        DeliveryAttempts++;
        NextDeliveryAttemptAt = utcNow.AddSeconds(Math.Min(3600, Math.Pow(2, Math.Min(DeliveryAttempts, 11)) * 5));
    }
}
