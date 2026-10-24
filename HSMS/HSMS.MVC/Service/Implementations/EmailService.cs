using System.Net;
using System.Net.Mail;

namespace HSMS.MVC.Service;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPortStr = _configuration["EmailSettings:SmtpPort"];
        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var senderPassword = _configuration["EmailSettings:SenderPassword"];
        var senderName = _configuration["EmailSettings:SenderName"] ?? "High School Management System";
        var enableSsl = bool.TryParse(_configuration["EmailSettings:EnableSsl"], out var ssl) ? ssl : true;

        if (!string.IsNullOrWhiteSpace(smtpHost) &&
            int.TryParse(smtpPortStr, out var smtpPort) &&
            !string.IsNullOrWhiteSpace(senderEmail) &&
            !string.IsNullOrWhiteSpace(senderPassword))
        {
            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "HSMS - Đặt lại mật khẩu tài khoản",
                    Body = $@"
                        <h3>Yêu cầu đặt lại mật khẩu</h3>
                        <p>Bạn nhận được email này vì đã có yêu cầu đặt lại mật khẩu cho tài khoản tại High School Management System.</p>
                        <p>Vui lòng nhấn vào liên kết bên dưới để tạo mật khẩu mới (liên kết có hiệu lực trong 15 phút):</p>
                        <p><a href=""{resetLink}"" style=""padding: 10px 18px; background-color: #2563EB; color: white; text-decoration: none; border-radius: 6px;"">Đặt lại mật khẩu</a></p>
                        <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Chỉ log khi có lỗi SMTP thực tế để phục vụ điều tra cấu hình máy chủ mail
                _logger.LogError(ex, "Lỗi khi gửi email SMTP đặt lại mật khẩu đến: {Email}", toEmail);
            }
        }
    }
}
