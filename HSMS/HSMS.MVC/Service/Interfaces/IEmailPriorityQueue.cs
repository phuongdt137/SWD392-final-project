using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Email;

namespace HSMS.MVC.Service.Interfaces;

/// <summary>
/// Hàng đợi ưu tiên bất đồng bộ cho hệ thống gửi Email chạy nền.
/// </summary>
public interface IEmailPriorityQueue
{
    /// <summary>
    /// Đưa một gói tin email vào hàng đợi theo mức độ ưu tiên.
    /// </summary>
    void Enqueue(EmailMessage message);

    /// <summary>
    /// Đưa nhanh một email vào hàng đợi với độ ưu tiên chỉ định.
    /// </summary>
    void Enqueue(
        string toEmail,
        string subject,
        string body,
        EmailPriority priority = EmailPriority.Normal,
        bool isHtml = true);

    /// <summary>
    /// Chờ và lấy ra gói tin email có độ ưu tiên cao nhất (giá trị enum nhỏ nhất).
    /// </summary>
    ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Số lượng email đang chờ trong hàng đợi.
    /// </summary>
    int Count { get; }
}
