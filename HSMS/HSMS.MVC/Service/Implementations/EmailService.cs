using System.Net;
using System.Net.Mail;
using System.Text;

namespace HSMS.MVC.Service;

// Dùng chung SMTP cho đặt lại mật khẩu và điểm danh, không tạo dịch vụ mail riêng.
public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        // Giữ cách xử lý của Auth: thiếu cấu hình thì bỏ qua, lỗi SMTP chỉ ghi log.
        if (!HasSmtpConfiguration()) return;
        try
        {
            var link = WebUtility.HtmlEncode(resetLink);
            var body = $"""
                <h3>Yêu cầu đặt lại mật khẩu</h3>
                <p>Bạn nhận được email này vì đã có yêu cầu đặt lại mật khẩu cho tài khoản tại High School Management System.</p>
                <p>Vui lòng nhấn vào liên kết bên dưới để tạo mật khẩu mới (liên kết có hiệu lực trong 15 phút):</p>
                <p><a href="{link}">Đặt lại mật khẩu</a></p>
                <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
                """;
            await SendHtmlAsync(toEmail, "HSMS - Đặt lại mật khẩu tài khoản", body, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi gửi email SMTP đặt lại mật khẩu đến: {Email}", toEmail);
        }
    }

    public async Task SendAttendanceEmailAsync(string toEmail, string parentName, string title, string content,
        string? relativeLink, CancellationToken ct = default)
    {
        // Dispatcher phải biết lỗi để cập nhật MessageLog và thử lại, không báo gửi thành công giả.
        if (!MailAddress.TryCreate(toEmail, out var address) ||
            address.Host.Equals("invalid", StringComparison.OrdinalIgnoreCase) ||
            address.Host.EndsWith(".invalid", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Phụ huynh chưa có địa chỉ email thực tế hợp lệ.");

        // Mã hóa tên/nội dung trước khi chèn vào HTML; ghi chú có thể do người dùng nhập.
        var name = WebUtility.HtmlEncode(parentName);
        var heading = WebUtility.HtmlEncode(title);
        var text = WebUtility.HtmlEncode(content).Replace("\n", "<br />");
        var link = AttendanceLink(relativeLink);
        var action = link == null ? "<p>Vui lòng đăng nhập HSMS để xem chi tiết chuyên cần.</p>" :
            $"<p><a href=\"{WebUtility.HtmlEncode(link)}\">Xem chi tiết chuyên cần</a></p>";
        var body = $"""
            <h3>{heading}</h3>
            <p>Kính gửi phụ huynh {name},</p>
            <p>{text}</p>
            {action}
            <p>Đây là thông báo tự động từ High School Management System.</p>
            """;
        await SendHtmlAsync(address.Address, "HSMS - " + title, body, ct);
    }

    private string? AttendanceLink(string? relativeLink)
    {
        // URL công khai là cấu hình triển khai, không lấy từ Host header của request.
        var configured = configuration["EmailSettings:ApplicationUrl"];
        if (!Uri.TryCreate(configured, UriKind.Absolute, out var app) ||
            (app.Scheme != Uri.UriSchemeHttps && app.Scheme != Uri.UriSchemeHttp) ||
            string.IsNullOrEmpty(relativeLink) || !relativeLink.StartsWith('/') ||
            !Uri.TryCreate(app, relativeLink, out var target) || target.Authority != app.Authority ||
            target.Scheme != app.Scheme) return null;
        return target.AbsoluteUri;
    }

    private bool HasSmtpConfiguration() =>
        !string.IsNullOrWhiteSpace(configuration["EmailSettings:SmtpHost"]) &&
        int.TryParse(configuration["EmailSettings:SmtpPort"], out var port) && port is > 0 and <= 65535 &&
        !string.IsNullOrWhiteSpace(configuration["EmailSettings:SenderEmail"]) &&
        !string.IsNullOrWhiteSpace(configuration["EmailSettings:SenderPassword"]);

    private async Task SendHtmlAsync(string recipient, string subject, string body, CancellationToken ct)
    {
        if (!HasSmtpConfiguration())
            throw new InvalidOperationException("Chưa cấu hình đầy đủ EmailSettings cho máy chủ SMTP.");

        using var client = new SmtpClient(configuration["EmailSettings:SmtpHost"],
            int.Parse(configuration["EmailSettings:SmtpPort"]!))
        {
            Credentials = new NetworkCredential(configuration["EmailSettings:SenderEmail"],
                configuration["EmailSettings:SenderPassword"]),
            EnableSsl = !bool.TryParse(configuration["EmailSettings:EnableSsl"], out var ssl) || ssl
        };
        using var message = new MailMessage
        {
            From = new MailAddress(configuration["EmailSettings:SenderEmail"]!,
                configuration["EmailSettings:SenderName"] ?? "High School Management System"),
            Subject = subject,
            SubjectEncoding = Encoding.UTF8,
            Body = body,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };
        message.To.Add(recipient);
        // Giới hạn thời gian mỗi lần gửi; token dừng ứng dụng vẫn có hiệu lực.
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        await client.SendMailAsync(message, timeout.Token);
    }
}
