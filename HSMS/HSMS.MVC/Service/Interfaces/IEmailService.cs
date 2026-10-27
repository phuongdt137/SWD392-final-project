namespace HSMS.MVC.Service;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    // Gửi email Attendance qua SMTP có sẵn; lỗi phải được trả cho tác vụ nền để thử lại.
    Task SendAttendanceEmailAsync(string toEmail, string parentName, string title, string content,
        string? relativeLink, CancellationToken ct = default);
}

