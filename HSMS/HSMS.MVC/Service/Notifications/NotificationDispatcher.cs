using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Notifications;
using HSMS.MVC.Hubs;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace HSMS.MVC.Service.Notifications;

/// <summary>Single-host durable delivery: database rows survive restarts and send failures.</summary>
public sealed class NotificationDispatcher(IServiceScopeFactory scopes, IHubContext<NotificationHub> hub,
    NotificationDispatchSignal signal, SchoolTime time, ILogger<NotificationDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var rows = await repo.DueAsync(time.UtcNow, stoppingToken);
                foreach (var n in rows)
                {
                    try
                    {
                        if (await repo.CanDeliverAsync(n, stoppingToken))
                            await hub.Clients.User("CUSTOMER:" + n.RecipientCustomerId).SendAsync("AttendanceNotification",
                                new NotificationItem(n.Id, n.Title, n.Content, n.IsRead, n.SentAt, n.Link), stoppingToken);
                        // A removed parent relationship must not receive an old queued payload.
                        n.Delivered(time.UtcNow);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    { n.RetryLater(time.UtcNow); logger.LogWarning(ex, "Notification {Id} delivery will retry", n.Id); }
                    await repo.SaveAsync(stoppingToken);
                }
                if (rows.Count == 50) signal.Wake();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogWarning(ex, "Notification dispatcher will retry; check Attendance migration is installed"); }
            await signal.WaitAsync(stoppingToken);
        }
    }
}
