using HSMS.MVC.Common;

namespace HSMS.MVC.DTOs.Email;

/// <summary>
/// Gói tin email đưa vào hàng đợi ưu tiên (PriorityQueue) để xử lý bất đồng bộ dưới nền.
/// </summary>
public sealed record EmailMessage(
    string ToEmail,
    string Subject,
    string Body,
    bool IsHtml = true,
    EmailPriority Priority = EmailPriority.Normal,
    long? RecipientUserId = null,
    string? TemplateCode = null
);
