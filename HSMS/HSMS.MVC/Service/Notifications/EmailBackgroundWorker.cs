using HSMS.MVC.Service.Interfaces;

namespace HSMS.MVC.Service.Notifications;

/// <summary>
/// Worker chạy nền liên tục lấy email từ IEmailPriorityQueue theo thứ tự ưu tiên (Critical -> High -> Normal -> Low)
/// và gửi qua IEmailService bất đồng bộ với HTTP Request.
/// </summary>
public sealed class EmailBackgroundWorker(
    IEmailPriorityQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<EmailBackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EmailBackgroundWorker đã khởi động, sẵn sàng xử lý hàng đợi ưu tiên.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Chờ bất đồng bộ tới khi có email trong hàng đợi và lấy email có Priority cao nhất (số enum nhỏ nhất)
                var message = await queue.DequeueAsync(stoppingToken);

                using var scope = scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                await emailService.SendEmailAsync(message, stoppingToken);

                logger.LogInformation(
                    "Đã gửi email [{Priority}] tới {ToEmail} - Chủ đề: {Subject} (Còn lại trong queue: {Remaining})",
                    message.Priority,
                    message.ToEmail,
                    message.Subject,
                    queue.Count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi xử lý gửi email trong EmailBackgroundWorker.");
            }
        }
    }
}
