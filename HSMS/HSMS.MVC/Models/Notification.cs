using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

/// <summary>
/// Thực thể Thông báo trong hệ thống.
/// </summary>
public class Notification
{
    public long Id { get; private set; }
    public long? SenderId { get; private set; }
    public long RecipientId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTime SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

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

    public bool SendNotification()
    {
        SentAt = DateTime.UtcNow;
        IsRead = false;
        return true;
    }

    public bool MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        return true;
    }
}
