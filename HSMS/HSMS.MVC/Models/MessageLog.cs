using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

public class MessageLog
{
    public long Id { get; private set; }
    public string Channel { get; private set; } = null!;
    public string? Provider { get; private set; }
    public long? RecipientCustomerId { get; private set; }
    public long? RecipientEmployeeId { get; private set; }
    public string RecipientAddress { get; private set; } = null!;
    public string? Subject { get; private set; }
    public string MessageBody { get; private set; } = null!;
    public string? TemplateCode { get; private set; }
    public string Status { get; private set; } = "QUEUED";
    public string? ExternalMessageId { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public decimal? Cost { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public long? CreatedByEmployeeId { get; private set; }
    public long? NotificationId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual Customer? RecipientCustomer { get; private set; }
    public virtual Employee? RecipientEmployee { get; private set; }
    public virtual Employee? CreatedByEmployee { get; private set; }
    public virtual Notification? Notification { get; private set; }

    protected MessageLog() { }

    public MessageLog(
        string channel,
        string recipientAddress,
        string messageBody,
        string? subject = null,
        string? provider = null,
        string? templateCode = null,
        long? recipientCustomerId = null,
        long? recipientEmployeeId = null,
        long? createdByEmployeeId = null,
        long? notificationId = null,
        decimal? cost = null)
    {
        if (string.IsNullOrWhiteSpace(channel))
            throw new AppError("Kênh gửi tin nhắn không được để trống.", "CHANNEL_REQUIRED");

        if (string.IsNullOrWhiteSpace(recipientAddress))
            throw new AppError("Địa chỉ nhận tin không được để trống.", "RECIPIENT_ADDRESS_REQUIRED");

        if (string.IsNullOrWhiteSpace(messageBody))
            throw new AppError("Nội dung tin nhắn không được để trống.", "MESSAGE_BODY_REQUIRED");

        if (recipientCustomerId.HasValue && recipientEmployeeId.HasValue)
            throw new AppError("Người nhận không thể đồng thời là Customer và Employee.", "INVALID_RECIPIENT");

        if (recipientCustomerId.HasValue && recipientCustomerId.Value <= 0)
            throw new AppError("Id Customer nhận tin không hợp lệ.", "INVALID_CUSTOMER_ID");

        if (recipientEmployeeId.HasValue && recipientEmployeeId.Value <= 0)
            throw new AppError("Id Employee nhận tin không hợp lệ.", "INVALID_EMPLOYEE_ID");

        if (createdByEmployeeId.HasValue && createdByEmployeeId.Value <= 0)
            throw new AppError("Id người tạo tin không hợp lệ.", "INVALID_CREATOR_ID");

        if (notificationId.HasValue && notificationId.Value <= 0)
            throw new AppError("Id thông báo liên kết không hợp lệ.", "INVALID_NOTIFICATION_ID");

        if (cost.HasValue && cost.Value < 0)
            throw new AppError("Chi phí gửi tin không được âm.", "INVALID_COST");

        Channel = channel.Trim().ToUpperInvariant();
        RecipientAddress = recipientAddress.Trim();
        MessageBody = messageBody.Trim();
        Subject = subject?.Trim();
        Provider = provider?.Trim();
        TemplateCode = templateCode?.Trim();
        RecipientCustomerId = recipientCustomerId;
        RecipientEmployeeId = recipientEmployeeId;
        CreatedByEmployeeId = createdByEmployeeId;
        NotificationId = notificationId;
        Cost = cost;
        Status = "QUEUED";
        RetryCount = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsSending()
    {
        Status = "SENDING";
    }

    public void MarkAsSent(string? externalMessageId = null, decimal? cost = null)
    {
        Status = "SENT";
        SentAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(externalMessageId))
            ExternalMessageId = externalMessageId.Trim();
        if (cost.HasValue)
            Cost = cost.Value;
    }

    public void MarkAsDelivered(DateTime? deliveredAt = null)
    {
        Status = "DELIVERED";
        DeliveredAt = deliveredAt ?? DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new AppError("Thông báo lỗi không được để trống.", "ERROR_MESSAGE_REQUIRED");

        Status = "FAILED";
        ErrorMessage = errorMessage.Trim();
    }

    public void PrepareRetry()
    {
        RetryCount++;
        Status = "QUEUED";
        ErrorMessage = null;
    }
}
