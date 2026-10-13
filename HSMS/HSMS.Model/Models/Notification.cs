using HSMS.Model.Exceptions;

namespace HSMS.Model.Model;

public class Notification
{
    public long Id { get; private set; }
    public long? SenderCustomerId { get; private set; }
    public long? SenderEmployeeId { get; private set; }
    public long? RecipientCustomerId { get; private set; }
    public long? RecipientEmployeeId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTime SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public virtual Customer? SenderCustomer { get; private set; }
    public virtual Employee? SenderEmployee { get; private set; }
    public virtual Customer? RecipientCustomer { get; private set; }
    public virtual Employee? RecipientEmployee { get; private set; }

    protected Notification() { }

    public Notification(string title, string content, long? senderCustomerId = null, long? senderEmployeeId = null, long? recipientCustomerId = null, long? recipientEmployeeId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessException("Tiêu đề thông báo không được để trống.", "TITLE_REQUIRED");

        if (string.IsNullOrWhiteSpace(content))
            throw new BusinessException("Nội dung thông báo không được để trống.", "CONTENT_REQUIRED");

        if (senderCustomerId.HasValue && senderEmployeeId.HasValue)
            throw new BusinessException("Người gửi không thể đồng thời là Customer và Employee.", "INVALID_SENDER");

        if (!((recipientCustomerId.HasValue && !recipientEmployeeId.HasValue) || (!recipientCustomerId.HasValue && recipientEmployeeId.HasValue)))
            throw new BusinessException("Người nhận phải là chính xác một Customer hoặc một Employee.", "INVALID_RECIPIENT");

        Title = title.Trim();
        Content = content.Trim();
        SenderCustomerId = senderCustomerId;
        SenderEmployeeId = senderEmployeeId;
        RecipientCustomerId = recipientCustomerId;
        RecipientEmployeeId = recipientEmployeeId;
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
