using HSMS.MVC.DTOs.Email;

namespace HSMS.MVC.Service;

public interface IEmailService
{
    /// <summary>
    /// Gửi trực tiếp một gói tin EmailMessage qua SMTP (dùng bởi EmailBackgroundWorker).
    /// </summary>
    Task SendEmailAsync(EmailMessage message, CancellationToken ct = default);

    /// <summary>
    /// Gửi email HTML tổng quát qua SMTP.
    /// </summary>
    Task SendHtmlEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);

    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);

    // Gửi email Attendance qua SMTP có sẵn; lỗi phải được trả cho tác vụ nền để thử lại.
    Task SendAttendanceEmailAsync(string toEmail, string parentName, string title, string content,
        string? relativeLink, CancellationToken ct = default);
}
