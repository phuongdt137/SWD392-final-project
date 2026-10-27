using HSMS.MVC.Common;
using HSMS.MVC.Repositories.Interfaces;

namespace HSMS.MVC.Service.Notifications;

// Hàng chờ Notification/MessageLog có sẵn: gửi email sau commit, không có kết nối real-time.
// Dành cho một máy chủ; SMTP lỗi không làm mất thông báo trong ứng dụng hoặc kết quả điểm danh.
public sealed class NotificationDispatcher(IServiceScopeFactory scopes, NotificationDispatchSignal signal,
    SchoolTime time, ILogger<NotificationDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var rows = await repo.DueAsync(time.UtcNow, stoppingToken);
                foreach (var notification in rows)
                {
                    var email = notification.MessageLogs.Single(m => m.Channel == "EMAIL" && m.TemplateCode == "ATTENDANCE");
                    try
                    {
                        // Kiểm tra lại liên kết con/tài khoản/role trước gửi, tránh lộ thông báo cũ.
                        if (!await repo.CanDeliverAsync(notification, stoppingToken))
                        {
                            email.MarkAsFailed("Người nhận không còn quyền nhận thông báo điểm danh này.");
                            notification.Delivered(time.UtcNow);
                        }
                        else
                        {
                            if (email.Status != "QUEUED") email.PrepareRetry();
                            email.UpdateRecipientAddress(notification.Recipient.Email);
                            email.MarkAsSending();
                            await repo.SaveAsync(stoppingToken);

                            await emailService.SendAttendanceEmailAsync(email.RecipientAddress,
                                notification.Recipient.FullName, notification.Title, notification.Content,
                                notification.Link, stoppingToken);
                            // SENT nghĩa SMTP đã chấp nhận thư, không phải phụ huynh đã đọc email.
                            email.MarkAsSent();
                            notification.Delivered(time.UtcNow);
                        }
                    }
                    catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                    {
                        email.MarkAsFailed("Gửi email chưa thành công; hệ thống sẽ thử lại.");
                        notification.RetryLater(time.UtcNow);
                        logger.LogWarning(ex, "Attendance email for notification {Id} will retry", notification.Id);
                    }
                    await repo.SaveAsync(stoppingToken);
                }
                if (rows.Count == 50) signal.Wake();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogWarning(ex, "Attendance email dispatcher will retry"); }
            await signal.WaitAsync(stoppingToken);
        }
    }
}
