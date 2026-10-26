using HSMS.MVC.Data;
using HSMS.MVC.DTOs.Notifications;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations;

public sealed class NotificationRepository(AppDbContext db) : INotificationRepository
{
    private IQueryable<Notification> Visible(long userId) => db.Notifications.Where(n => n.RecipientId == userId &&
        (n.AttendanceAuditLogId == null || (n.AttendanceAuditLog!.AttendanceRecord.Customer.Parent != null &&
        n.AttendanceAuditLog.AttendanceRecord.Customer.Parent.UserId == userId &&
        n.Recipient.IsActive && n.Recipient.UserRoles.Any(r => r.IsActive && r.Role.IsActive && r.Role.RoleCode == "PARENT"))));
    public async Task<NotificationFeed> FeedAsync(long userId, CancellationToken ct)
    {
        var q = Visible(userId).AsNoTracking();
        var unread = await q.CountAsync(x => !x.IsRead, ct);
        var items = await q.OrderByDescending(x => x.SentAt).ThenByDescending(x => x.Id).Take(100)
            .Select(x => new NotificationItem(x.Id, x.Title, x.Content, x.IsRead, x.SentAt, x.Link)).ToListAsync(ct);
        return new(unread, items);
    }
    public async Task<bool> ReadAsync(long userId, long id, CancellationToken ct)
    {
        var item = await Visible(userId).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item == null) return false;
        if (!item.IsRead) { item.MarkAsRead(); await db.SaveChangesAsync(ct); }
        return true;
    }
    public Task<List<Notification>> DueAsync(DateTime utcNow, CancellationToken ct) => db.Notifications
        .Where(x => x.AttendanceAuditLogId != null && x.DispatchedAt == null && (x.NextDeliveryAttemptAt == null || x.NextDeliveryAttemptAt <= utcNow))
        .OrderBy(x => x.Id).Take(50).ToListAsync(ct);
    public Task<bool> CanDeliverAsync(Notification notification, CancellationToken ct) => Visible(notification.RecipientId).AnyAsync(x => x.Id == notification.Id, ct);
    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
